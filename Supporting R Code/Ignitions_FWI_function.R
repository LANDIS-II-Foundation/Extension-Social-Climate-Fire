##Quick function to translate fire weather index into number of ignitions
##Will produce integers of number of fires per day based on input FWI value
##Alec Kretchun, Portland State University 2017
##No support provided

### TO DO: COMBINE FWI RECORDS (FWI.DAT AND FWI.DAT.RECENT)


#Load things
library(raster)
library(foreign)
library(plyr)
#library(dplyr)
library(likelihood)
require(pscl)
library(boot)


##Getting dates from Karen Short fire database
#Set up directories
w.dir <- "I:/LANDIS_code/2017FireExt/FireHistoryData/"
out.dir <- "I:/SNPLMA3/LANDIS_modeling/LTW_NECN_H/ScrappleInputs/"

#Read in historical fire data
##Need to do all 3 fire ignition types
#Lightning
l.fire.dat <- read.dbf(paste(w.dir, "KarenShort_Lightning.dbf", sep=""))
l.fire.days <- as.data.frame(cbind(l.fire.dat$FIRE_YEAR, l.fire.dat$DISCOVERY1)) #Extracting year and julian day
colnames(l.fire.days) <- c("YEAR", "J_DAY")
l.fire.days.sort <- l.fire.days[order(l.fire.days[,1]),] #sorting by year

#plot no of fires/yr
l.fire.days.sort.count<- cbind(l.fire.days.sort, rep(1, nrow(l.fire.days.sort)))
l.fires.count <- cbind.data.frame(unique(l.fire.days.sort.count$YEAR) , 
                                 tapply(l.fire.days.sort.count$`rep(1, nrow(l.fire.days.sort))`,l.fire.days.sort.count$YEAR, sum))
colnames(l.fires.count) <- c("YEAR", "COUNT")
barplot(l.fires.count$COUNT, main ="No of ign/yr Lightning")

#Human Accidental
h.fire.dat <- read.dbf(paste(w.dir, "KarenShort_HumanAccidental.dbf", sep=""))
h.fire.days <- as.data.frame(cbind(h.fire.dat$FIRE_YEAR, h.fire.dat$DISCOVERY1)) #Extracting year and julian day
colnames(h.fire.days) <- c("YEAR", "J_DAY")
h.fire.days.sort <- h.fire.days[order(h.fire.days[,1]),] #sorting by year
#plot no of fires/yr
h.fire.days.sort.count<- cbind(h.fire.days.sort, rep(1, nrow(h.fire.days.sort)))
h.fires.count <- cbind.data.frame(unique(h.fire.days.sort.count$YEAR) , 
                                  tapply(h.fire.days.sort.count$`rep(1, nrow(h.fire.days.sort))`,h.fire.days.sort.count$YEAR, sum))
colnames(h.fires.count) <- c("YEAR", "COUNT")
barplot(h.fires.count$COUNT, main ="No of ign/yr Human Accidental")

#RX
r.fire.dat <- read.dbf(paste(w.dir, "Rx_Fires.dbf", sep=""))
r.date <- strptime(r.fire.dat$DATE_ACCOM, format = "%Y-%m-%d")
r.julian <- r.date$yday
r.year <- r.date$year + 1900
r.fire.days <- as.data.frame(cbind(r.year, r.julian))
colnames(r.fire.days) <- c("YEAR", "J_DAY")
r.fire.days.sort <- r.fire.days[order(r.fire.days[,1]),] #sorting by year
r.fire.days.sort <- r.fire.days.sort[complete.cases(r.fire.days.sort),] #removing NAs

#plot no of fires/yr
r.fire.days.sort.count<- cbind(r.fire.days.sort, rep(1, nrow(r.fire.days.sort)))
r.fires.count <- cbind.data.frame(unique(r.fire.days.sort.count$YEAR) , 
                                  tapply(r.fire.days.sort.count$`rep(1, nrow(r.fire.days.sort))`,r.fire.days.sort.count$YEAR, sum))
colnames(r.fires.count) <- c("YEAR", "COUNT")
barplot(r.fires.count$COUNT)



ign.types <- c("Lightning", "HumanAccidental", "RxBurn")

fire.days.list <- list(l.fire.days.sort, h.fire.days.sort, r.fire.days.sort) ##organizing all ignition types into a list

##Import daily historic FWI data
FWI.dat <- read.csv("I:/LANDIS_code/2017FireExt/FireHistoryData/Climate-future-input-log.csv")
FWI.dat <- FWI.dat[,c(1,2,13)]
FWI.dat$ID <- paste(FWI.dat$Year, "_", FWI.dat$Timestep, sep="") #creating date identifier out of date and julian day

FWI.dat.recent <- read.csv("I:/LANDIS_code/2017FireExt/FireHistoryData/Climate-future-input-log2005_2016.csv")
FWI.dat.recent <- FWI.dat.recent[,c(1,2,13)]
FWI.dat.recent$ID <- paste(FWI.dat.recent$Year, "_", FWI.dat.recent$Timestep, sep="") #creating date identifier out of date and julian day

##Loop through ignition type data to calculate number of fires/day and corresponding FWI
##Find days with multiple fires
igns.list <- list()
for (i in 1:length(ign.types[1:2])){ ##THIS DOESN'T INCLUDE RX BURNS BUT THATS CAUSE WE ARE PROVIDING THOSE TO SCRAPPLE DIRECT
  ign.type.select <- fire.days.list[[i]] ##selecting each ignition type individually
  fire.days.count <- ddply(ign.type.select, .(ign.type.select$YEAR, ign.type.select$J_DAY), nrow) #finds duplicate rows in fire data
  colnames(fire.days.count) <- c("YEAR", "JD", "No_FIRES") #Renaming columns for consistency
  fire.days.count$ID <- paste(fire.days.count$YEAR, "_", fire.days.count$JD, sep="") #creating date identifier out of date and julian day

  ##Merging dataframes by year and julian day
  fire.days.short <- subset(fire.days.count, fire.days.count$YEAR < 2014) ##restricting fire records to climate file years
  FWI.short <- subset(FWI.dat, FWI.dat$Year > 1991) #restricting climate data to fire history records
  merge.col <- FWI.short$ID
  FWI.fire.merge <- join(FWI.short, fire.days.short, type="left") ##Merging based on unique date id
  FWI.fire.number <- FWI.fire.merge[,c(1,3,7)] #pulling out FWI and number of fires
  FWI.fire.number[is.na(FWI.fire.number)] <- 0 #converting NAs to 0, meaning 0 fires
  plot(FWI.fire.number[,1], FWI.fire.number[,2], main =ign.types[i], xlab = "dailyFWI", ylab = "noFires") #plotting FWI against no of fires just to look at pattern
  igns.list[[i]] <- FWI.fire.number 
}
rm(i)

summary(igns.list[[1]])
summary(igns.list[[2]])

##Fitting regression for daily number of fires ~ FWI
##Trying a 0-inflated Poisson distribution
zeroinf.mod <- zeroinfl(igns.list[[1]][,3]~igns.list[[1]][,2], dist="poisson", data=igns.list[[2]])
summary(zeroinf.mod)

#Evaluating the coefficients
e <- exp(1) #for creating e
test <- e^(-3+ (.05*igns.list[[2]][,2])) ##Testing the coefficients produced by the zero-inflated model
plot(test, ylab = "number of fires", xlab = "day", main = "Predicted human accidental ignitions")


##Fitting regression for annual number of fires ~ FWI
lightnings<- igns.list[[1]]
yrs <- lightnings$Year
fwis <- lightnings$FWI 
fires <- lightnings$No_FIRES
avg.fwi <- tapply(fwis, yrs, mean)
total.no.fires <- tapply(fires, yrs, sum)

lightnings.annual <- cbind.data.frame(unique(yrs), avg.fwi, total.no.fires)
l.mod <- zeroinfl(lightnings.annual[,3]~lightnings.annual[,2], data = lightnings.annual)
summary(l.mod)

test <- e^(-10 + (0.005*lightnings.annual[,2])) ##Testing the coefficients produced by the zero-inflated model
plot(test, ylab = "number of fires", xlab = "day", main = "Predicted human accidental ignitions")  

##Need to redo the above loop for Rx fires, because I'm using a different time series
###THIS REUSES SEVERAL VARIABLES ABOVE SO PAY ATTENTION #########
ign.type.select <- r.fire.days.sort ##selecting each ignition type individually
fire.days.count <- ddply(ign.type.select, .(ign.type.select$YEAR, ign.type.select$J_DAY), nrow) #finds duplicate rows in fire data
colnames(fire.days.count) <- c("YEAR", "JD", "No_FIRES") #Renaming columns for consistency
fire.days.count$ID <- paste(fire.days.count$YEAR, "_", fire.days.count$JD, sep="") #creating date identifier out of date and julian day

##Merging dataframes by year and julian day
fire.days.short <- subset(fire.days.count, fire.days.count$YEAR < 2017) ##restricting fire records to climate file years
#FWI.short <- subset(FWI.dat.recent, FWI.dat$Year > 1991) #restricting climate data to fire history records
merge.col <- FWI.dat.recent$ID
FWI.fire.merge <- join(FWI.dat.recent, fire.days.short, type="left") ##Merging based on unique date id
FWI.fire.number <- FWI.fire.merge[,c(3,7)] #pulling out FWI and number of fires
FWI.fire.number[is.na(FWI.fire.number)] <- 0 #converting NAs to 0, meaning 0 fires
plot(FWI.fire.number[,1], FWI.fire.number[,2], main = "Rx burns", xlab = "dailyFWI", ylab = "noFires") #plotting FWI against no of fires just to look at pattern

## doing some additional analysis cause rx fires are unique to the other 2 categories ##
rx.burns <- FWI.fire.number[(FWI.fire.number[,2] >0),]
summary(rx.burns) #summary stats for rx burn FWI conditions

##comparing FWI values of fires by type
l.fire <- igns.list[[1]]
l.FWIs <- l.fire[,1]
h.fire <- igns.list[[2]]
h.FWIs <- h.fire[,1]
fire.FWIs <- cbind(rx.burns[,1], l.FWIs, h.FWIs)

par(mfrow=c(1,3))
boxplot(rx.burns[,1])
boxplot(l.FWIs)
boxplot(h.FWIs)


