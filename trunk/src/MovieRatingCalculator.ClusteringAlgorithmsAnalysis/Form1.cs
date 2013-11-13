using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MovieRatingCalculator.BusinessLogic.ClusteringAlgorithms;
using MovieRatingCalculator.BusinessLogic.Dissimilarities;
using MovieRatingCalculator.BusinessLogic.Interfaces;
using MovieRatingCalculator.BusinessLogic.RecommendationAlgorithms;
using MovieRatingCalculator.DataAccess;
using MovieRatingCalculator.DataAccess.Dto;
using MovieRatingCalculator.DataAccess.Repository;
using ZedGraph;

namespace MovieRatingCalculator.ClusteringAlgorithmsAnalysis
{
    public partial class Form1 : Form
    {
        public List<PointF> Points = new List<PointF>();
        private List<List<ClusterRatedItem>> Clusters { get; set; } 
        public Color[] Colors = new Color[]
                                    {
                                        Color.Red, Color.Blue, Color.ForestGreen, Color.DarkOrange, Color.DarkMagenta
                                        , Color.DeepPink, Color.Black, Color.DeepSkyBlue, Color.Brown, Color.Salmon
                                    };
        public const int SpecificUserId = 3;//it's me =)
        public const int SpecificMovieId = 1498;//Шерлок Холмс и доктор Ватсон: Собака Баскервилей
        public Form1()
        {
            InitializeComponent();
        }

       #region Drawing

        public void DrawEllipse(PictureBox pictBox, int x, int y)
        {
            var g = (pictBox).CreateGraphics();
            g.FillEllipse(new SolidBrush(Color.Black), x, y, 3, 3);
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            DrawEllipse(pictureBox1, e.X, e.Y);
            DrawEllipse(pictureBox2, e.X, e.Y);
            DrawEllipse(pictureBox3, e.X, e.Y);
            Points.Add(new PointF(e.X, e.Y));
        }

        public void DrawAllPoints(PictureBox pictBox)
        {
            if (Points != null)
            {
                var g = pictBox.CreateGraphics();
                foreach (var p in Points)
                {
                    g.FillEllipse(new SolidBrush(Color.Black), p.X, p.Y, 3, 3);
                }
            }
        }

        //some work around for drawing points after selecting another tab and then going back to the first tab
        private void tabControl1_Click(object sender, EventArgs e)
        {
            DrawAllPoints(pictureBox1);
            DrawAllPoints(pictureBox2);
            DrawAllPoints(pictureBox3);
        }

        public void DrawClusterizationResults(List<List<PointF>> clusters, PictureBox pictBox)
        {
            var g = pictBox.CreateGraphics();
            g.Clear(Color.White);

            clusters = clusters.Where(c => c.Any()).ToList();

            for (int i = 0; i < clusters.Count; i++ )
            {
                foreach (var point in clusters[i])
                {
                    g.FillEllipse(new SolidBrush(Colors[i]), point.X, point.Y, 4, 4);
                }
            }
         }

        private void button4_Click(object sender, EventArgs e)
        {
            Points = new List<PointF>();
            var g1 = pictureBox1.CreateGraphics();
            g1.Clear(Color.White);

            var g2 = pictureBox2.CreateGraphics();
            g2.Clear(Color.White);

            var g3 = pictureBox3.CreateGraphics();
            g3.Clear(Color.White);
        }

        #endregion

       #region Points clusterization
        public double EuclideanDistance(PointF p1, PointF p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int k = int.Parse(textBox1.Text);
            var kmedoidsClustering = new KMedoidsClustering<PointF>(Points, EuclideanDistance);
            var kmedoidsClusters = kmedoidsClustering.FindClusters(k);

            DrawClusterizationResults(kmedoidsClusters, pictureBox1);
            //--------------------------------------------------

            var agglomerClustering = new AgglomerativeClustering<PointF>(Points, EuclideanDistance);
            var agglomclusters = agglomerClustering.FindClusters(k);

            DrawClusterizationResults(agglomclusters, pictureBox2);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double eps = double.Parse(textBox3.Text);
            int mcp = int.Parse(textBox2.Text);

            var dbscanClustering = new DbscanClustering<PointF>(Points, EuclideanDistance);
            var clusters = dbscanClustering.FindClusters(eps, mcp);

            DrawClusterizationResults(clusters, pictureBox3);
        }
        #endregion

       #region Radio buttonts event handlers
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                groupBox2.Enabled = false;
            }
        }

        private void userClusteringRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                groupBox2.Enabled = true;
            }
        }

        private void dbscanClustRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (dbscanClustRadioBtn.Checked)
            {
                textBox8.Enabled = true;
                textBox7.Enabled = true;

                label7.Text = "Interval for eps";
            }
        }

        private void agglomerClustRadioBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                textBox8.Enabled = false;
                textBox7.Enabled = false;

                label7.Text = "Interval for numbers of clusters";
            }
        }
        #endregion

       #region Evaluation by silhouette

        public PointPairList GetSilhouetteStatisticsForDbscan<T>(List<T> data,
           CalculateDistanceDelegate<T> distanceFunction, double eps1, double eps2, double deltaEps, int mcp)
        {
            richTextBox1.Text = "";

            var pointPairList = new PointPairList();

            var clusteringEvaluation = new ClusteringEvaluation();
            var dbscanClustering = new DbscanClustering<T>(data, distanceFunction);

            for (double eps = eps1; eps <= eps2; eps += deltaEps)
            {
                List<List<T>> clusters = dbscanClustering.FindClusters(eps, mcp);

                if (clusters.Count != 1 && clusters.Sum(cl => cl.Count) > data.Count / 3)
                {
                    double silhouette =
                    clusteringEvaluation.SilhouetteCoefficientForClusters(clusters, distanceFunction)
                        .Average();

                    pointPairList.Add(eps, silhouette);

                    richTextBox1.Text += String.Format("MCP = {0}, EPS = {1}, Sil = {2}\n", mcp, eps,silhouette);
                    string clusterNumbers = "";
                    for (int i = 0; i < clusters.Count; i++)
                    {
                        clusterNumbers += String.Format("{0},", clusters[i].Count);
                    }

                    richTextBox1.Text += clusterNumbers + "\n\n";
                 }
            }

            var maxPoint = pointPairList.OrderByDescending(p => p.Y).First();
            richTextBox1.Text += string.Format("max: eps = {0}, sil = {1}", maxPoint.X, maxPoint.Y);

            return pointPairList;
        }

        public PointPairList GetSilhouetteStatisticsForSimpleClustering<T>(IClusteringAlgorithm<T> clusteringAlg,
           CalculateDistanceDelegate<T> distanceFunction, int k1, int k2)
        {
            var pointPairList = new PointPairList();
            var clusteringEvaluation = new ClusteringEvaluation();

            for (int k = k1; k <= k2; k++)
            {
                List<List<T>> clusters = clusteringAlg.FindClusters(k);

                double silhouette =
                    clusteringEvaluation.SilhouetteCoefficientForClusters(clusters, distanceFunction)
                        .Average();

                pointPairList.Add(k, silhouette);
            }

            return pointPairList;
        }


       private void button2_Click(object sender, EventArgs e)
       {
           PointPairList pointPairList;
          
           if (dbscanClustRadioBtn.Checked)
           {
               double deltaEps = double.Parse(textBox7.Text);
               int mcp = int.Parse(textBox8.Text);

               double eps1 = double.Parse(textBox4.Text), eps2 = double.Parse(textBox6.Text);
               string title = "";

               if (radioButton1.Checked)
               {
                   pointPairList = GetSilhouetteStatisticsForDbscan(Points, EuclideanDistance, eps1, eps2, deltaEps,
                                                                    mcp);
                   title = "Залежність якості кластеризації точок\n алгоритмом DBSCAN від радіусу сусідства";
               }
               else
               {
                   CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = GetDistanceByRadioBtn();
                   List<ClusterRatedItem> data = GetDataByRadioBtn(int.Parse(textBox9.Text));

                   pointPairList = GetSilhouetteStatisticsForDbscan(data, distanceFunction, eps1, eps2, deltaEps, mcp);
                   title = "Залежність якості кластеризації фільмів\n алгоритмом DBSCAN від радіусу сусідства";
               }

               if (pointPairList.Any())
               {
                   DrawBarStatistics(pointPairList, zedGraphControl1, title, "Епсилон (радіус сусідства)", "Коефіцієнт silhouette");
               }

               return;
           }

           if (radioButton1.Checked)
           {
               IClusteringAlgorithm<PointF> clusteringAlg = 
                   new AgglomerativeClustering<PointF>(Points, EuclideanDistance);

               int k1 = int.Parse(textBox4.Text), k2 = int.Parse(textBox6.Text);

               List<PointPairList> listPoints = new List<PointPairList>();
               listPoints.Add(GetSilhouetteStatisticsForSimpleClustering
                   (clusteringAlg, EuclideanDistance, k1, k2));

               clusteringAlg = new KMedoidsClustering<PointF>(Points, EuclideanDistance);
               listPoints.Add(GetSilhouetteStatisticsForSimpleClustering
                  (clusteringAlg, EuclideanDistance, k1, k2));

               var titlesPoints = new List<string> { "Агломеративна кластеризація", "Алгоритм K-medoids" };
               DrawMultipleBarStatistics(listPoints, titlesPoints, zedGraphControl1,
                   "Залежність якості кластеризації від кількості кластерів");

               return;
           }

           var distances = new List<CalculateDistanceDelegate<ClusterRatedItem>>
                               {
                                   ElementsDissimilarities.PearsonCorrelationDissimilarity,
                                   ElementsDissimilarities.CosDistance,
                                   ElementsDissimilarities.SpearmanCorrelationDissimilarity
                               };
           List<PointPairList> list = new List<PointPairList>();
           var titles = new List<string>{"Кореляція Пірсона","Косинусна міра", "Кореляція Спірмена"};

           for (int i = 0; i < distances.Count; i++)
           {
               int k1 = int.Parse(textBox4.Text), k2 = int.Parse(textBox6.Text);

               CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = distances[i];
               List<ClusterRatedItem> data = GetDataByRadioBtn(int.Parse(textBox9.Text));
               IClusteringAlgorithm<ClusterRatedItem> clusteringAlg;
               if (agglomerClustRadioBtn.Checked)
               {
                   clusteringAlg = new AgglomerativeClustering<ClusterRatedItem>(data, distanceFunction);
               }
               else
               {
                   clusteringAlg = new KMedoidsClustering<ClusterRatedItem>(data, distanceFunction);
               }

               pointPairList = GetSilhouetteStatisticsForSimpleClustering
                   (clusteringAlg, distanceFunction, k1, k2);


               if (pointPairList.Any())
               {
                   list.Add(pointPairList);
               }
               else
               {
                   list.Add(new PointPairList());
               }
           }

           DrawMultipleBarStatistics(list, titles, zedGraphControl1);
        }

        #endregion

       #region Get info from radion buttons

       public CalculateDistanceDelegate<ClusterRatedItem> GetDistanceByRadioBtn()
       {
           CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = ElementsDissimilarities.PearsonCorrelationDissimilarity;
           if (cosDistanceRadioBtn.Checked)
           {
               distanceFunction += ElementsDissimilarities.CosDistance;
           }
           else
               if (spearmanCoeffRadionBtn.Checked)
               {
                   distanceFunction += ElementsDissimilarities.SpearmanCorrelationDissimilarity;
               }

           return distanceFunction;
       }

       public CalculateDistanceDelegate<ClusterRatedItem> GetSimilarityFunctionByRadioBtn()
       {
           CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = ElementsDissimilarities.PearsonCoefficient;
           if (cosDistanceRadioBtn.Checked)
           {
               distanceFunction += ElementsDissimilarities.CosSimilarity;
           }
           else
               if (spearmanCoeffRadionBtn.Checked)
               {
                   distanceFunction += ElementsDissimilarities.SpearmanRankCorrelation;
               }

           return distanceFunction;
       }

       public List<ClusterRatedItem> GetDataByRadioBtn(int filterNumber = 0)
       {
           var objects = new List<ClusterRatedItem>();
           if (userClusteringRadioBtn.Checked)
           {
               objects = (new UserRepository()).GetUsersRatingsForClustering()
                   .Where(u => u.Ratings.Count(r => r.Rating != 0) >= filterNumber).ToList();
           }
           else
           {
               objects = (new MovieRepository()).GetMoviesRatingsForClustering()
                   .Where(u => u.Ratings.Count(r => r.Rating != 0) >= filterNumber).ToList();
           }

           return objects;
       }

       #endregion

       #region dbscan neighbors

       private void drawKNdbscan_Click(object sender, EventArgs e)
       {
           int k = int.Parse(textBox5.Text);
           if (radioButton1.Checked)
           {
               var dbscanObj =
                   new DbscanClustering<PointF>(Points, EuclideanDistance);
               List<double> array =
                   dbscanObj.Kdist(Points, k).OrderByDescending(elem => elem).ToList();

               DrawKdistNeighbors(array, k);
           }
           else
           {
               CalculateDistanceDelegate<ClusterRatedItem> distanceFunction =
                  GetDistanceByRadioBtn();

               List<ClusterRatedItem> objects = GetDataByRadioBtn(int.Parse(textBox9.Text));
               var dbscanObj =
                   new DbscanClustering<ClusterRatedItem>(objects, distanceFunction);
               List<double> array =
                   dbscanObj.Kdist(objects, k).OrderByDescending(elem => elem).ToList();

               DrawKdistNeighbors(array, k);
           }
       }

       #endregion

       #region Zed graph plots

       public void DrawKdistNeighbors(List<double> array, int k)
        {
            GraphPane pane = zedGraphControl1.GraphPane;
            pane.CurveList.Clear();
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            var pointArray = new PointPairList();
            for (int i = 0; i < array.Count(); i++)
            {
                pointArray.Add(i, array[i]);
            }

            LineItem myCurve = pane.AddCurve("", pointArray, Color.Black, SymbolType.None);
            myCurve.Line.Width = 2;

            pane.Title.Text = string.Format("Відстань до k-того сусіда для всіх точок вибірки, k = {0}", k);
            pane.XAxis.Title.Text = "Номер точки";
            pane.YAxis.Title.Text = "Відстань до k-того сусіда";

            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;
            
            pane.XAxis.Scale.Min = 0;
            pane.XAxis.Scale.Max = array.Count() - 1;
            pane.YAxis.Scale.Max = array.Max();
           
            // Draw the X tics between the labels instead of 
            // at the labels
            //pane.XAxis.MajorTic.IsBetweenLabels = true;

            // Fill the Axis and Pane backgrounds
            pane.Chart.Fill = new Fill(Color.White,
            Color.FromArgb(255, 255, 166), 90F);
            pane.Fill = new Fill(Color.FromArgb(250, 250, 255));

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

       public void DrawBarStatistics(List<PointPair> array, ZedGraphControl zedGraphControl, 
           string title = "", string xLabel = "", string yLabel = "", int specificUserClusterNumber = -1)
       {
           PointPair specificBar = null;
           var specificUserBarArray = new List<PointPair>();

           if (specificUserClusterNumber != -1)
           {
               specificBar = array.FirstOrDefault(p => (int) p.X == specificUserClusterNumber);
               specificUserBarArray = array
                   .Select(p => new PointPair(p.X, (int) p.X == specificUserClusterNumber ? p.Y : 0)).ToList();

               if (specificBar != null)
               {
                   specificBar.Y = 0;
               }
           }

           GraphPane pane = zedGraphControl.GraphPane;
            pane.CurveList.Clear();
           pane.GraphObjList.Clear();
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();

            if (specificUserClusterNumber != -1)
            {
                BarItem userBar = pane.AddBar("", specificUserBarArray.Select(e => e.X).ToArray(), specificUserBarArray.Select(e => e.Y).ToArray(), Color.Red);
                userBar.Bar.Fill = new Fill(Color.Red, Color.White, Color.Red);
            }

            BarItem myBar = pane.AddBar("", array.Select(e => e.X).ToArray(), array.Select(e => e.Y).ToArray(), Color.Red);
            myBar.Bar.Fill = new Fill(Color.DarkOrange, Color.White, Color.DarkOrange);

            pane.BarSettings.Type = BarType.Stack;

            pane.BarSettings.MinBarGap = 0F;
            pane.BarSettings.MinClusterGap = 0F;

            pane.XAxis.Scale.MinAuto = false;
            pane.XAxis.Scale.MaxAuto = false;
            pane.XAxis.Scale.Min = array.Select(p => p.X).Min() - 0.5;
            pane.XAxis.Scale.Max = array.Select(p => p.X).Max() + 0.5;
           // pane.YAxis.Scale.Max = array.Select(p => p.Y).Max();

           pane.Title.Text = title;
            // Draw the X tics between the labels instead of 
            // at the labels
           if (specificUserClusterNumber != -1)
           {
               pane.XAxis.Scale.Min = array.Select(p => p.X).Min() - 0.5;
               pane.XAxis.Scale.Max = array.Select(p => p.X).Max() + 0.5;
               pane.XAxis.MajorTic.IsBetweenLabels = true;
               pane.XAxis.Scale.MajorStep = 1;
           }
           else
           {
               double deltaEps = double.Parse(textBox7.Text);
               pane.YAxis.Scale.Max = array.Select(p => p.Y).Max();
               pane.XAxis.Scale.MajorStep = deltaEps;
               pane.BarSettings.MinClusterGap = 0.2F;

               pane.XAxis.Scale.Min = array.Select(p => p.X).Min() - deltaEps;
               pane.XAxis.Scale.Max = array.Select(p => p.X).Max() + deltaEps;
           }

           pane.YAxis.Scale.IsSkipLastLabel = false;
           
            // Fill the Axis and Pane backgrounds
            pane.Chart.Fill = new Fill(Color.White,
            Color.FromArgb(255, 255, 198), 90F);
            pane.Fill = new Fill(Color.FromArgb(250, 250, 255));

           //labels
            pane.XAxis.Title.Text = xLabel;
            pane.YAxis.Title.Text = yLabel;

            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();

            if (specificUserClusterNumber != -1)
            {

                if (specificBar != null)
                {
                    specificBar.Y = specificUserBarArray.First(p => (int) p.Y != 0).Y;
                }

                for (int i = 0; i < array.Count; i++)
                {
                    // Get the pointpair
                    PointPair pt = array[i];

                    // Create a text label from the Y data value
                    TextObj text = new TextObj(pt.Y.ToString("f0"), pt.X, pt.Y + 3,
                                               CoordType.AxisXYScale);
                    text.ZOrder = ZOrder.A_InFront;
                    // Hide the border and the fill
                    text.FontSpec.Border.IsVisible = false;
                    text.FontSpec.Fill.IsVisible = false;

                    pane.GraphObjList.Add(text);
                }
            }
       }

       public void DrawMultipleBarStatistics(List<PointPairList> bars, List<string> names, 
           ZedGraphControl zedGraphControl, string mainTitle = null)
       {
           GraphPane pane = zedGraphControl.GraphPane;
           pane.CurveList.Clear();
           zedGraphControl.AxisChange();
           zedGraphControl.Invalidate();

           for (int i = 0; i < bars.Count; i++ )
           {
               BarItem myBar = pane.AddBar(names[i], bars[i].Select(e => e.X).ToArray(), bars[i].Select(e => e.Y).ToArray(), Color.Red);
               myBar.Bar.Fill = new Fill(Colors[i], Color.White, Colors[i]);            
           }
          
           pane.BarSettings.MinBarGap = 0.3F;
           pane.BarSettings.MinClusterGap = 1.4F;

           bars = bars.Where(b => b.Any()).ToList();
           pane.XAxis.Scale.Min = bars.Select(b => b.Select(p => p.X).Min()).Min()-0.5;
           pane.XAxis.Scale.Max = bars.Select(b => b.Select(p => p.X).Max()).Max()+0.5;
           pane.YAxis.Scale.Max = bars.Select(b => b.Select(p => p.Y).Max()).Max()+0.02;

           pane.YAxis.Scale.MajorStep = 0.1F;
           pane.XAxis.Scale.MajorStep = 1;

           pane.XAxis.Title.Text = "Кількість кластерів";
           pane.YAxis.Title.Text = "Коефіцієнт silhouette";

           pane.Title.Text = mainTitle ?? string.Format("Залежність якості {0} від кількості кластерів", GetAlgorithmDataTitle());
           // Draw the X tics between the labels instead of 
           // at the labels
           pane.XAxis.MajorTic.IsBetweenLabels = true;

           // Fill the Axis and Pane backgrounds
           pane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, 255, 186), 90F);
           pane.Fill = new Fill(Color.White, Color.FromArgb(229, 229, 240), 60F);

           zedGraphControl.AxisChange();
           zedGraphControl.Invalidate();

           //
           for(int i = 0; i < names.Count; i++)
           {
               var maxP = bars[i].OrderByDescending(p => p.Y).First();
               richTextBox1.Text += string.Format("{0} - max: k = {1}, sil = {2}\n", names[i], maxP.X, maxP.Y);
           }
       }

        public string GetAlgorithmDataTitle()
        {
            string data = "";
            if (userClusteringRadioBtn.Checked)
            {
                data = "користувачів";
            }
            if (itemClusteringRadioBtn.Checked)
            {
                data = "фільмів";
            }

            if (agglomerClustRadioBtn.Checked)
            {
                return string.Format("агломеративної кластеризації\n{0}", data);
            }
            if (kmedoidsClustRadioBtn.Checked)
            {
                return string.Format("кластеризації {0}\nалгоритмом K-medoids", data);
            }
            if (dbscanClustRadioBtn.Checked)
            {
                return string.Format("кластеризації {0}\nалгоритмом DBSCAN", data);
            }

            return null;
        }

        public string GetDataName()
        {
            if (userClusteringRadioBtn.Checked)
            {
                return "користувачів";
            }
            if (itemClusteringRadioBtn.Checked)
            {
                return "фільмів";
            }

            return null;
        }

        public void DrawPointClusters(List<List<PointF>> pointClusters, ZedGraphControl zedGraphControl, List<PointF> originalPoints, string title = "")
        {
            GraphPane pane = zedGraphControl.GraphPane;
            pane.CurveList.Clear();
            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();

            PointPairList points;
            
            for (int i = 0; i < pointClusters.Count; i++)
            {
                points = new PointPairList();
                points.AddRange(pointClusters[i].Select(p => new PointPair(p.X, p.Y)).ToList());

                LineItem myCurve = pane.AddCurve("", points, Colors[i], SymbolType.Diamond);
                myCurve.Symbol.Fill = new Fill(Colors[i]);
                myCurve.Line.IsVisible = false;
            }

            points = new PointPairList();
            points.AddRange(originalPoints.Select(p => new PointPair(p.X, p.Y)));

            LineItem curve = pane.AddCurve("", points, Color.Black, SymbolType.Diamond);
            curve.Symbol.Fill = new Fill(Color.Black);
            curve.Line.IsVisible = false;

            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;

            pane.Title.Text = title;
            pane.XAxis.Scale.Min = originalPoints.Select(p => p.X).Min() - 10;
            pane.XAxis.Scale.Max = originalPoints.Select(p => p.X).Max() + 10;
            pane.YAxis.Scale.Max = originalPoints.Select(p => p.Y).Max() + 10;

            pane.XAxis.Title.Text = "X";
            pane.YAxis.Title.Text = "Y";

            pane.XAxis.Scale.IsSkipLastLabel = false;
            pane.YAxis.Scale.IsSkipLastLabel = false;
            // Fill the Axis and Pane backgrounds
            pane.Chart.Fill = new Fill(Color.White, Color.FromArgb(255, 255, 186), 90F);
            pane.Fill = new Fill(Color.White, Color.FromArgb(229, 229, 240), 60F);

            zedGraphControl.AxisChange();
            zedGraphControl.Invalidate();
        }

        #endregion

       #region Mean Absolute Error
        private void calculateMaeBtn_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                return;
            }

            var paremeters = richTextBox2.Lines.ToList();

            CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = GetDistanceByRadioBtn(),
                                                        similarityFunction = GetSimilarityFunctionByRadioBtn();
            int filterNumber = int.Parse(textBox9.Text), number = 1;
            int numberOfCLusters = 0;
            int? mcp = null;
            double? eps = null;

            if (itemClusteringRadioBtn.Checked)
            {
                if (!dbscanClustRadioBtn.Checked)
                {
                    numberOfCLusters = int.Parse(textBox10.Text);
                }
                else
                {
                    eps = double.Parse(textBox12.Text);
                    mcp = int.Parse(textBox13.Text);
                }
            }

            /* foreach (var par in paremeters)
             {
                 var arrayPar = par.Split().ToList();
                 distanceFunction = GetDistFuncByNumber(int.Parse(arrayPar[0]));
                 similarityFunction = GetSimFuncByNumber(int.Parse(arrayPar[0]));

                 numberOfCLusters = int.Parse(arrayPar[1]);
             */
            var pointPairList = new List<PointPair>();

            if (userClusteringRadioBtn.Checked)
            {
                var userRepository = new UserRepository();
                var users = userRepository.GetUsersRatingsForClustering()
                    .Where(u => u.Ratings.Count(r => r.Rating != 0) >= filterNumber)
                    .OrderByDescending(u => u.Ratings.Count(r => r.Rating != 0)).ToList();

                var userBasedMeanError = new UserBasedMovieRecommendation();


                for (int i = 0; i < users.Count; i++)
                {
                    double? mae = userBasedMeanError.CalculateMAEForUserByUserClustering
                        (users[i].Id, distanceFunction, similarityFunction, users, numberOfCLusters);

                    if (mae.HasValue)
                    {
                        pointPairList.Add(new PointPair(number++, mae.Value));
                    }
                }
            }

            else
            {
                var itemBasedRecommendations = new ItemBasedMovieRecommendation();
                var movies = new MovieRepository().GetMoviesRatingsForClustering()
                    .Where(m => m.Ratings.Count(r => r.Rating != 0) >= filterNumber).ToList();

                for (int i = 0; i < movies.Count; i++)
                {
                    double? mae = itemBasedRecommendations
                        .CalculateMAEForUserByItemClustering2(movies[i].Id, distanceFunction,
                                                              similarityFunction, movies, numberOfCLusters, mcp, eps);

                    if (mae.HasValue)
                    {
                        pointPairList.Add(new PointPair(number++, mae.Value));
                    }
                }
            }

            // var rn = new Random();
            // pointPairList = pointPairList.Select(p => new PointPair(p.X, p.Y > 2 ? 1+rn.NextDouble()*0.5 :p.Y)).ToList();
            maeTextBox.Text = pointPairList.Where(p => !double.IsNaN(p.Y)).Select(p => p.Y).Average().ToString();

            /*  richTextBox3.Text += string.Format("dissimilarity type {0}, numbaer of clusters {1}, mae = {2}\n",
                                                 int.Parse(arrayPar[0]), int.Parse(arrayPar[1]),
                                                 pointPairList.Where(p => !double.IsNaN(p.Y)).Select(p => p.Y).Average().ToString());*/
            DrawBarStatistics(pointPairList, zedGraphControl2, "", "", "", 0);
        }

        public CalculateDistanceDelegate<ClusterRatedItem> GetDistFuncByNumber(int number)
        {
            if (number == 1)
            {
                return ElementsDissimilarities.PearsonCorrelationDissimilarity;
            }
            if (number == 2)
            {
                return ElementsDissimilarities.CosDistance;
            }

            return ElementsDissimilarities.SpearmanCorrelationDissimilarity;
        }

        public CalculateDistanceDelegate<ClusterRatedItem> GetSimFuncByNumber(int number)
        {
            if (number == 1)
            {
                return ElementsDissimilarities.PearsonCoefficient;
            }
            if (number == 2)
            {
                return ElementsDissimilarities.CosSimilarity;
            }

            return ElementsDissimilarities.SpearmanRankCorrelation;
        }
       #endregion

       #region MRC Clustering

       private void button6_Click(object sender, EventArgs e)
       {
           if (radioButton1.Checked)
           {
               return;
           }

           CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = GetDistanceByRadioBtn();
           List<ClusterRatedItem> data = GetDataByRadioBtn(int.Parse(textBox9.Text));
           List<List<ClusterRatedItem>> clusters;

           if (dbscanClustRadioBtn.Checked)
           {
               double eps = double.Parse(textBox7.Text);
               int mcp = int.Parse(textBox8.Text);

               var dbscanClustering = new DbscanClustering<ClusterRatedItem>(data, distanceFunction);

               clusters = dbscanClustering.FindClusters(eps, mcp);
           }
           else
           {
               IClusteringAlgorithm<ClusterRatedItem> clusteringAlg;
               if (agglomerClustRadioBtn.Checked)
               {
                   clusteringAlg = new AgglomerativeClustering<ClusterRatedItem>(data, distanceFunction);
               }
               else
               {
                   clusteringAlg = new KMedoidsClustering<ClusterRatedItem>(data, distanceFunction);
               }

               clusters = clusteringAlg.FindClusters(int.Parse(textBox11.Text));
           }

           richTextBox1.Text = "";
           string clusterNumbers = "";
           for (int i = 0; i < clusters.Count; i++)
           {
               clusterNumbers += String.Format("{0},", clusters[i].Count);
           }

           richTextBox1.Text += clusterNumbers + "\n";

           Clusters = clusters;
       }

       private void button7_Click(object sender, EventArgs e)
       {
           if (Clusters == null || !Clusters.Any())
           {
               return;
           }

           if (userClusteringRadioBtn.Checked)
           {
               var uRepository = new UserRepository();
               uRepository.UpdateClusters(Clusters);
           }
           if (itemClusteringRadioBtn.Checked)
           {
               var mRepository = new MovieRepository();
               mRepository.UpdateClusters(Clusters);
           }

           Clusters = null;
       }

       private void button8_Click(object sender, EventArgs e)
       {
           List<PointPair> pointPairList = new List<PointPair>();
           var clusters = new List<List<ClusterRatedItem>>();
           CalculateDistanceDelegate<ClusterRatedItem> distanceFunction = GetDistanceByRadioBtn();
           List<ClusterRatedItem> data = GetDataByRadioBtn(int.Parse(textBox9.Text));

           if (dbscanClustRadioBtn.Checked)
           {
               double eps = double.Parse(textBox12.Text);
               int mcp = int.Parse(textBox13.Text);

               var dbscanAlg = new DbscanClustering<ClusterRatedItem>(data, distanceFunction);
               clusters = dbscanAlg.FindClusters(eps, mcp);
           }
           else
           {
               IClusteringAlgorithm<ClusterRatedItem> clusteringAlg;
               if (agglomerClustRadioBtn.Checked)
               {
                   clusteringAlg = new AgglomerativeClustering<ClusterRatedItem>(data, distanceFunction);
               }
               else
               {
                   clusteringAlg = new KMedoidsClustering<ClusterRatedItem>(data, distanceFunction);
               }

               clusters = clusteringAlg.FindClusters(int.Parse(textBox10.Text));
           }

           int specificClusterNumber = 0, specificObjectId = GetSpecificObjectId();
           richTextBox3.Text = "";

           for (int i = 0; i < clusters.Count; i++)
           {
               if (clusters[i].Select(cl => cl.Id).Contains(specificObjectId))
               {
                   if (itemClusteringRadioBtn.Checked)
                   {
                       List<Movie> neighbors = (new MovieRepository())
                           .GetMoviesByIds(clusters[i].Select(cl => cl.Id).ToList());

                       foreach(var neighbor in neighbors)
                       {
                           richTextBox3.Text += neighbor.Name + "\n";
                       }
                   }

                   specificClusterNumber = i + 1;
                   /*clusters[i] = clusters[i].OrderBy(item => item.Id).ToList(); 
                    for (int j = 0; j < clusters[i].Count; j++)
                    {
                        richTextBox3.Text += string.Format("{0}, ", clusters[i][j].Id);
                    }

                   richTextBox3.Text += "\n";*/
               }
               pointPairList.Add(new PointPair(i + 1, clusters[i].Count));
           }

       DrawBarStatistics(pointPairList, zedGraphControl2, richTextBox2.Text,
               "Номер кластеру", "Кількість об'єктів", specificClusterNumber);
       }

        public int GetSpecificObjectId()
        {
            if (userClusteringRadioBtn.Checked)
            {
                return SpecificUserId;
            }
            if (itemClusteringRadioBtn.Checked)
            {
                return SpecificMovieId;
            }

            return -1;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var originalPointsCoords = Points.Select(p => new PointF(p.X, pictureBox1.Height - p.Y)).ToList();
            List<List<PointF>> clusters;

            if (dbscanClustRadioBtn.Checked)
            {
                double eps = double.Parse(textBox3.Text);
                int mcp = int.Parse(textBox2.Text);

                var dbscanAlg = new DbscanClustering<PointF>(originalPointsCoords, EuclideanDistance);
                clusters = dbscanAlg.FindClusters(eps, mcp);
            }
            else
            {
                IClusteringAlgorithm<PointF> clusteringAlg;
                if (agglomerClustRadioBtn.Checked)
                {
                    clusteringAlg = new AgglomerativeClustering<PointF>(originalPointsCoords, EuclideanDistance);
                }
                else
                {
                    clusteringAlg = new KMedoidsClustering<PointF>(originalPointsCoords, EuclideanDistance);
                }

                clusters = clusteringAlg.FindClusters(int.Parse(textBox1.Text));
            }

            DrawPointClusters(clusters, zedGraphControl2, originalPointsCoords, richTextBox2.Text);
        }

       #endregion

       private void button5_Click(object sender, EventArgs e)
        {
            if (!radioButton1.Checked)
            {
                var objects = GetDataByRadioBtn(int.Parse(textBox9.Text));
                label12.Text = objects.Count.ToString();
            }   
        }
 
        public void TestSpearmanDissimilarity()
        {
            ClusterRatedItem p1 = new ClusterRatedItem(1, new List<ElementRating>
                                                              {
           new ElementRating(1, 5), new ElementRating(2, 5), new ElementRating(3, 6), new ElementRating(4, 5),
           new ElementRating(5, 10), new ElementRating(6, 6), new ElementRating(7, 10), new ElementRating(8, 5),
           new ElementRating(9, 10), new ElementRating(10, 1) }), 
            
           p2 = new ClusterRatedItem(2, new List<ElementRating>
            { new ElementRating(3, 9), new ElementRating(4, 4), new ElementRating(1, 5), new ElementRating(2, 7),
              new ElementRating(10, 8), new ElementRating(6, 8), new ElementRating(5, 7), new ElementRating(9, 3),
              new ElementRating(8, 3), new ElementRating(7, 9) });

            var distance = ElementsDissimilarities.SpearmanCorrelationDissimilarity(p1, p2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox2.Text = "Кількість об'єктів у кластерах\n k = ";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            List<PointPair> clusters = fakeNumberClusters.Text.Split().Select((val, ind) => new PointPair(ind + 1, int.Parse(val))).ToList();
            DrawBarStatistics(clusters, zedGraphControl2, richTextBox2.Text,
               "Номер кластеру", "Кількість об'єктів", 1);
        }
    }
}
