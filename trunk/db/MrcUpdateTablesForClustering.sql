use MovieRatingCalculator
go

alter table Movies 
add ClusterId smallint NULL

alter table Users
add ClusterId smallint NULL