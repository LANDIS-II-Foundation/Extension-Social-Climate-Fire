##This script is proof of concept for processing fire perimeter data, in order to extract pertinent spread factors
##Its supposed to extract climate values from rasters that overlap with fire perimeters at a daily basis

##ULTIMATELY WE NEED A MATRIX WITH DATE,FWI, WSV, WD, FINEFUELS, T/F (SPREAD/NO SPREAD) FOR EVERY PERIMETER CELL

##NEED TO FIGURE OUT THE RIGHT METHOD FOR FITTING 5 PARAMERS LOGISTIC EQUATION WITH A BINARY RESPONSE (SPREAD VS NO SPREAD)
##PACKAGE NPLR LOOKS PROMISING

##SHOULD WE ELIMINATE DAYS WITH NEGATIVE SPREADS?

##Alec Kretchun, 2017

library(maptools)
library(raster)
library(dplyr)
library(rgdal)
library(rgeos)
library(geosphere)
#library(SpaDES) #This is that package from the PFC in BC for doing spread(). Still figuring out if it will be useful
library(nplr)
library(pscl)

##Read things
##Climate data
wsv.dat <- read.csv("I:/LANDIS_code/2017FireExt/FireHistoryData/Climate/EPAIII_wsv.csv", skip=2)
##Directories
fire.dir <- "I:/LANDIS_code/2017FireExt/FireHistoryData/GeoMAC/"

#raster of watersheds, which will be tied to climate data
wind.map.old <- raster("I:/SNPLMA3/Data/GIS/CA_HUC08_largecell.tif")
wind.map <- raster("I:/SNPLMA3/Data/GIS/EPAIII.tif")
#wind.map <- projectRaster(wind.map, crs = crs(wind.map.old), res=res(wind.map.old)) ##project screwed up my values big time
wind.map <- projectRaster(wind.map, to = wind.map.old, method='ngb')

#raster of fuel types from LANDFIRE
fccs.raster <- raster("I:/LANDIS_code/2017FireExt/Fuels/FCCS_2001_extract1.tif") #raster of 2001 fuel beds
fccs.defs <- read.csv("I:/LANDIS_code/2017FireExt/Fuels/LF_consume.csv", skip=1) #definitions of fuelbed types

fuel.map <- projectRaster(fccs.raster, to = wind.map.old, method='ngb') #I chose nearest neoghbor for the fuels maps cause its a categorical variable

##Reading in uphill slope azimuth map
slope.azi.map <- raster("I:/LANDIS_code/2017FireExt/DEM/UphillSlopeAzimuth.img")
uphill.azi.map <- projectRaster(slope.azi.map, to = wind.map.old, method='bilinear')


##Stacking all rasters as a test
climate.stack <- stack(wind.map, fuel.map, uphill.azi.map)

#FWIs from SCRAPPLE
fwi.dat <- read.csv("I:/LANDIS_code/Testers/SCRAPPLE/FWI_getter/Climate-future-input-log.csv")


#Setting up names of fire perimeter shapefiles
years <- 2000:2016
perimeter.map.names <- list.files(fire.dir, full.names = TRUE)
perimeter.map.names <- grep(".shp", perimeter.map.names, value = TRUE) 
perimeter.map.names <- grep(".xml", perimeter.map.names, value=TRUE, invert=TRUE)

perimeter.map <- readShapePoly("I:/LANDIS_code/2017FireExt/FireHistoryData/GeoMAC/perims2012.shp",
                               proj4string = crs(wind.map)) #single year for now
#raster of watersheds to reference wsv
# wind.map <- raster("I:/SNPLMA3/Data/GIS/CA_HUC08_largecell.tif")

# crs(wind.map) <- crs.p
# extent(wind.map) <- ex.p

#Finding names of all fires. This will need to loop through years as well, but not implemented yet
fire.names <- perimeter.map$fire_name #This pulls out all unique fire names
##Select for fires that lasted more than 1 day
fire.names.manydays <- unique(fire.names[duplicated(fire.names)])

##MAY HAVE TO DO THE SAME THING FOR FIRE DATE AS WELL
firenames.cols <- c("firename", "firename", "event_name", "firename", "firename", "firename", "fire_name",
                    "fire_name", "fire_name", "fire_name", "fire_name", "fire_name", "fire_name", "fire_name",
                    "fire_name", "fire_name", "incidentna") ##This is a list of column names for fire names across all the years since they aren't consistent in GeoMAC

###################### Loop through year to select all individual fires and fire days ###########################
########### Loop through individual fire days to extract successful and unsuccessful spread cells ###############

## Basic workflow is:
##Select an individual fire
##Remove repeat days 
##Remove last day and fires that lasted only one day 
##Create wind perimeter from appropriate wind day raster 
##Extract values of successful spread 

#Empty objects and column names and stuff used in loops
# wind.fire.names <- c("FireDay", "FireDay1", "FireAdd", "WindSpeed", "Date")
# fuel.fire.names <- c("FireDay", "FireDay1", "FireAdd", "FuelType", "Date")
# azi.fire.names <- c("FireDay", "FireDay1", "FireAdd", "UphillAzi", "Date")
# fire.spread.mat <- NULL
# fueltype.mat <- NULL
# azi.mat <- NULL
climate.day.mat <- NULL
#dates.full <- NULL

#Extracting successful and unsuccessful spread
for(k in 1:length(perimeter.map.names)){
  perimeter.map <-readShapePoly(perimeter.map.names[k], proj4string = crs(wind.map)) #reading in annual fire perimeter maps 
  ##Need to rename attribute for firename for consistency across years
  names(perimeter.map)[names(perimeter.map) == firenames.cols[k]] <- "fire_name" ##renames attribute for fire name
  fire.names <- perimeter.map$fire_name #This pulls out all fire names
  ##Select for fires that lasted more than 1 day
  fire.names.manydays <- unique(fire.names[duplicated(fire.names)])
  fire.names.manydays <- fire.names.manydays[!is.na(fire.names.manydays)] ##Removing potential NAs
  
  for (i in 1:length(fire.names.manydays)){
    #fire.select <- perimeter.map[perimeter.map$fire_name == fire.names.manydays[i],] #selecting an individual fire. doesn't work 7/7
    fire.select <- subset(perimeter.map, perimeter.map$fire_name ==fire.names.manydays[i]) #selecting an individual fire
    fire.days <- sort(unique(fire.select$date_)) #sorting fire days into chronological order
    
    ##Add a short escape in case fire days <= 1
    if(length(fire.days) < 2) next
    
    for(j in 1:(length(fire.days)-1)){
      ##Selecting out individual fire perimeters
      fire.day.select <- subset(fire.select, fire.select$date_ == fire.days[j])# selecting the first day of the fire
      fire.day.select <- fire.day.select[1,] #selecting the first fire perim from that date, in case there are multiples
      fire.day.centroid <- gCentroid(fire.day.select) #Finding centroid for estimatign wind direction
      fire.day.select.1 <- subset(fire.select, fire.select$date_ == fire.days[j+1])#getting day 2 fire perimeter
      fire.day.select.1 <- fire.day.select.1[1,] #selecting the first fire perim from that date, in case there are multiples
      
      ##Getting wind direction. This is inferred from direction of spread, where aziumth of spread = wind direction
      fire.day.1.centroid <- gCentroid(fire.day.select.1) #Finding centroid for estimatign wind direction
      fire.azimuth <- bearingRhumb(fire.day.centroid, fire.day.1.centroid) #find the azimuth of fire spread by Rhumb bearing. This is used as a proxy assumed wind direction
      
      ##Getting area of spread by subtracting perimeter of day 1 from day 2
      perimeter.expansion <- perimeter(fire.day.select.1) - perimeter(fire.day.select)
      
      print(fire.day.select.1$date_)
      
      #Creating vector of fire dates. It doesnt work when I bind them below for some reason 
      date.char <- as.POSIXct(as.character(fire.day.select$date_), format = "%Y-%m-%d", tz="") 
      
      ##Extracting climate and fuels variables from raster stack.
      #This will compile a data frame of every cell that fire perimeters touch and whether 
      day1 <- extract(climate.stack, fire.day.select, cellnumbers = TRUE, df=TRUE, method="simple")
      day2 <- extract(climate.stack, fire.day.select.1, cellnumbers = TRUE, df=TRUE, method="simple")
      spread.success.inverse <- as.numeric(day2$cell %in% day1$cell) #Matching cell numbers to find spread and nonspread cells
      spread.success <- ifelse(spread.success.inverse == 0, 1,
                        ifelse(spread.success.inverse==1, 0, 0)) #reversing codes for spread. spread now == 1, nonspread == 0 
      #print(spread.success) #QAQC
      climate.day.df <- cbind(fire.day.select$fire_name, date.char, day2[,2:5], fire.azimuth,
                              perimeter.expansion, spread.success) #Putting everything into a big dataframe with all climate variables
      
      #dates <- climate.day.df[,2]
      #dates.full <- c(dates.full, dates)
      
      climate.day.mat <- rbind.data.frame(climate.day.mat, climate.day.df) #binding individual days to everyother day

    }
  }
}

rm(i) #removing loopers
rm(j)
rm(k)

#Removing rows with NA values
climate.day.complete <- climate.day.mat[complete.cases(climate.day.mat),]

##Attaching a unique ID to each row in case we need it later
climate.day.complete <- cbind(1:nrow(climate.day.complete), climate.day.complete)

##Renaming columns
colnames(climate.day.complete) <- c("ID", "fire_name", "date", "cell", "wind_region", "fuel_number", "uphill_azi", 
                               "fire_azimuth", "expansion", "spread")


################## PROCESSING EXTERNAL DATA SETS TO USE WITH EPA REGION AND CONSUME LOOK UP TABLES #####################
### THIS IS WHERE I TAKE CATEGORICAL VARIABLES FROM INPUT RASTERS AND FIND REAL VALUES OF WIND SPEED, FUEL BIOMASS ETC ######

##Process FWI values produced by LANDIS climate library to prep for attaching to larger spread matrix
##Need to attach ecoregion info
fwi.dat.slim <- fwi.dat[,c(1,2,3,13)]
fwi.date.info <- with(fwi.dat.slim, paste(Year, Timestep))
fwi.dates <- strptime(fwi.date.info, "%Y %j") #COnverting from julian day to y-m-d
fwi.date.dat <- cbind(as.POSIXct(fwi.dates), fwi.dat.slim[,3:4]) #attaching 
colnames(fwi.date.dat) <- c("Date", "Ecoregion", "FWI")


##Attach LANDFIRE fuel loading info based on fuel type number
##I selected columns I thought would define fine fuels, but these can change if we need. Units are tons/acre
fccs.loading <- fccs.defs[,c("fuelbed_number", "understory_loading",  "litter_loading", "lichen_loading")] ##removed "shrubs_primary_loading", "shrubs_secondary_loading",
fccs.loading.total <- rowSums(fccs.loading[,2:length(fccs.loading)])
fccs.loading.total <- fccs.loading.total * 224.17 #convert from tons/acre to g/m-2
fccs.finefuels <- cbind(fccs.loading[,1], fccs.loading.total)
colnames(fccs.finefuels) <- c("fueltype", "finefuels_loading")

##Linking up successful spread days with actual wind speed from GeoDataPortal-created csv
wsv.dat <- cbind.data.frame(as.POSIXct(wsv.dat[,1]), wsv.dat[,2:6])
colnames(wsv.dat) <- c("date", 13, 4, 5, 80, 9)

##Loop that attaches actual wind speed, FWI, and fuel biomass to successul spread cell days

climate.fuel.df <- matrix(nrow=nrow(climate.day.complete), ncol = 3)
for(i in 1:nrow(climate.day.complete)){
  fire.cell.select <- climate.day.complete[i,]
  print(fire.cell.select$date)
  #extracting real wind speed velocity value for this day, using EPA region as reference number
  wsv.day <- wsv.dat[grep(fire.cell.select$date, wsv.dat[,1]), match(fire.cell.select$wind_region, colnames(wsv.dat))]
  
  #extracting FWI value for they day based on 
  fwi.days <- fwi.date.dat[grep(fire.cell.select$date, fwi.date.dat[,1]),] #matching dates to climate library output
  fwi.day <- fwi.days[match(fire.cell.select$wind_region, fwi.days[,2]),3] #extracting fwi from correct ecoregion
  
  #extracting fuel biomass from FCCS class look up table
  fuel.day <- fccs.finefuels[match(fire.cell.select$fuel_number, fccs.finefuels[,1]),2]
  
  day.variables <- c(wsv.day, fwi.day, fuel.day)
  
  #Binding all variables to original dataframe of climate and fuel raster extractions
  climate.fuel.df[i,] <- day.variables #re-implement this is a blank matrix fill. binding takes forever
  
}
rm(i)

colnames(climate.fuel.df) <- c("Windspeed", "FWI", "finefuels")

climate.vars.df <- cbind.data.frame(climate.day.complete, climate.fuel.df)

##Converting uphill slope azimuth, wind azimuth, and wind speed to wind speed factor and wind direction factor

wsx <- (climate.vars.df$Windspeed * sin(climate.vars.df$fire_azimuth)) + (climate.vars.df$Windspeed * sin(climate.vars.df$uphill_azi))
wsy <- (climate.vars.df$Windspeed * cos(climate.vars.df$fire_azimuth)) + (climate.vars.df$Windspeed * cos(climate.vars.df$uphill_azi))

ws.factor <- sqrt(wsx^2 + wsy^2) #wind speed factor

wd.factor <- acos(wsy/ws.factor) #wind direction factor

spread.vars.complete <- cbind.data.frame(climate.vars.df, ws.factor, wd.factor)

######### FITTING FUNCTION OF CLIMATE AND FUELS VARIABLES TO DOSE RESPONSE, IE SPREAD VS NONSPREAD

##Stripping non-essential function fitting variables (fire name, date, etc)
spread.vars.short <- spread.vars.complete[,c(10, 12:15)]


##Exporting a csv of the fire spread variable database. This will get updated fairly frequently but I can overwrite as necessary
##it'll just save time when trying to do function fitting to have an external file to play with

write.csv(spread.vars.complete, file = "I:/LANDIS_code/2017FireExt/Spread/FireSpreadVars_Prelim.csv")

#### TRYING TO FIT A FXN ##########

#Trying out a simple linear model
 spread.mod <- lm(spread.vars.complete$spread ~ spread.vars.complete$FWI + spread.vars.complete$ws.factor + 
                    spread.vars.complete$wd.factor + spread.vars.complete$finefuels)
# 
# ##Trying out logistic regression
 spread.glm <- glm(spread~.,family=binomial(link='logit'), data = spread.vars.short)
 summary(spread.glm)
 anova(spread.glm)
 pR2(spread.glm)
 
# 
# #Trying out glm with spread >0
pos.spread.database <- spread.vars.complete[(spread.vars.complete$expansion) > 0,]
pos.spread.short <- pos.spread.database[,c(10, 12:15)]
 
pos.spread.glm <- glm(spread~.,gaussian(link = "identity"), data = pos.spread.short)
summary(pos.spread.glm)
anova(pos.spread.glm) 
 
#Trying out a simple linear model with positive spread
spread.mod <- lm(pos.spread.database$spread ~ pos.spread.database$FWI + pos.spread.database$ws.factor + 
                   pos.spread.database$wd.factor + pos.spread.database$finefuels)
 
summary(spread.mod)
 
#Using a zero-inflated poisson with just ws factor for exploratory purposes

zeroinf.mod <- zeroinfl(pos.spread.database$spread~pos.spread.database$ws.factor, dist="poisson", data=pos.spread.database)
summary(zeroinf.mod)


##Implementing 5 parameter logistic (5PL) function for spread data (Gottschalk and Dunn 2005)
parameter.vector <- c(100,.2,2,200,.2)#Vector of parameters a,b,c,d,g in 5PL fxn

a <- parameter.vector[1]
b <- parameter.vector[2]
c <- parameter.vector[3]
d <- parameter.vector[4]
g <- parameter.vector[5]

#constructing the 5PL. 
x <- c(1:10) 
y = d + ((a-d) / (1 + (x/c)^b)^g) #The form of the 5PM described in GOttshcalk and Dunn
plot(y, type="l")

##5PL continued. From GOttschalk "After the 5PL has been fitted to standard data, estimates for single-dilution unknown concentrations of
## x can be obtained from unknown responses y using the inverse. I'm not sure how to use any of this. Review Jeans paper I guess

x <- c(((a-d/y-d) ^ 1/g) - 1)^1/b

####################### Todds random forest modeling #########################################
# randomForest library - see documentation for variable importance and partial effects plots
# library(randomForest)	# very slow to fit models
# rf1 = randomForest(MTBS_flag ~ WUIFLAG + HDEN + POPDEN + ROADS + NLCD + lf_elevation + lf_slope + lf_aspect + NA_L2CODE + decade, x2); summary(rf1)
# 
# 
# spread.vars.short <- spread.vars.short[complete.cases(spread.vars.short$finefuels),]

# ranger library
library(ranger)	# fits models more quickly (~2 minutes), but could multithread if "thread" R library were available.  Try on a linux server

train.idx <- sample(x = 150, size = 100)
train.dat <- spread.vars.short[train.idx,]

r1 = ranger(spread ~ FWI + finefuels + ws.factor + wd.factor,
            data=train.dat, importance="permutation", write.forest=TRUE)
r1
importance(r1)
barplot(sort(importance(r1)))


test.dat <- spread.vars.short[-train.idx,]

r2 <- predict(r1, test.dat, type='response', verbose=TRUE)   #this will make predictions w/ new data based on random forest model produced above
plot(test.dat$ws.factor, predictions(r2), ylab="Predicted spread prob", 
     xlab='Subsample FWI', main = "Random Forest investigation")

# # randomForestSRC library
# library(parallel)
# library(randomForestSRC)	# also slow, try on linux.  Has better functionality to visualize RF model results
# options(rf.cores=detectCores()-4, mc.cores=detectCores()-4)
# cat(sprintf("Number of cores=%d\n", detectCores()-4))
# 
# x2$MTBS_flag = factor(x2$MTBS_flag)	# MTBS_flag needs to be a factor so rfsrc will do a classification
# 
# r1 = rfsrc(MTBS_flag ~ WUIFLAG + HDEN + POPDEN + ROADS + NLCD + lf_elevation + lf_slope + lf_aspect + NA_L2CODE + decade, data=x2, ntree=400, importance=TRUE, nodesize=100, nodedepth=7)
# r1
# 
# png("RF_plot.png",res=300,height=300*6,width=300*6,pointsize=12)
# plot(r1)
# dev.off()

#find.interaction(r1, c("HDEN","POPDEN","WUIFLAG","lf_elevation","lf_slope","lf_aspect","NA_L2CODE","NA_L3CODE","ROADS","WILDVEG","NLCD"))

# png("variable_partial_effects.png",res=300,height=300*7.5,width=300*10,pointsize=10)
# vars = c("HDEN","POPDEN","WUIFLAG","lf_elevation","lf_slope","lf_aspect","NA_L2CODE","NA_L3CODE","ROADS","WILDVEG","NLCD")
# plot.variable(r1, xvar.names=vars, which.class=2)
# dev.off()

 