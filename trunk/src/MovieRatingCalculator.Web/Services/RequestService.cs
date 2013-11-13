using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using MovieRatingCalculator.Web.Interfaces;

namespace MovieRatingCalculator.Web.Services
{
    public class RequestService : IRequestService
    {
        public string GetClientIpAddress(HttpRequestBase request)
        {
            try
            {
                var xForwardedFor = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrWhiteSpace(xForwardedFor))
                {
                    var publicIps = xForwardedFor.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                                                  Where(ip => !IsPrivateIpAddress(ip.Trim())).ToList();
                    if (publicIps.Any())
                    {
                        return publicIps.Last().Trim();
                    }
                }

                string remoteAddr = request.ServerVariables["REMOTE_ADDR"];
                if (!string.IsNullOrWhiteSpace(remoteAddr))
                {
                    return remoteAddr.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.UserHostAddress))
                {
                    return request.UserHostAddress;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool IsPrivateIpAddress(string ipAddress)
        {
            // Private IP Addresses: 
            //  24-bit block: 10.0.0.0 - 10.255.255.255
            //  20-bit block: 172.16.0.0 - 172.31.255.255
            //  16-bit block: 192.168.0.0 - 192.168.255.255
            //  Link-local addresses: 169.254.0.0 - 169.254.255.255
            
            var ip = IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();
            
            return (octets[0] == 10) || (octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31)
                   || (octets[0] == 192 && octets[1] == 168) || (octets[0] == 169 && octets[1] == 254);
        }
    }
}