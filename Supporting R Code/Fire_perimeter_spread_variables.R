##This script is proof of concept for processing fire perimeter data, in order to extract pertinent spread factors
##Its supposed to extract climate values from rasters that overlap with fire perimeters at a daily basis and intended to be shared on the SCRAPPLE repository


##Alec Kretchun, 2017

library(maptools)
library(raster)
library(dplyr)
library(rgdal)
library(rgeos)
library(geosphere)
library(nplr)
library(pscl)
library(ROCR)
library(likelihood)

##Read things
##Climate data
wsv.dat <- read.csv("I:/LANDIS_code/2017FireExt/FireHistoryData/Climate/EPAIII_wsv.csv", skip=2)
##Directories
fire.dir <- "I:/LANDIS_code/2017FireExt/FireHistoryData/GeoMAC/"


#raster of watersheds, which will be tied to climate data
wind.map.old <- raster("I:/SNPLMA3/Data/GIS/CA_HUC08_largecell.tif")
wind.map <- raster("I:/SNPLMA3/Data/GIS/EPAIII.tif")


#raster of fuel types from LANDFIRE
fccs.raster <- raster("I:/LANDIS_code/2017FireExt/Fuels/FCCS_2001_extract1.tif") #raster of 2001 fuel beds
fccs.defs <- read.csv("I:/LANDIS_code/2017FireExt/Fuels/LF_consume.csv", skip=1) #definitions of fuelbed types

#fuel.map <- projectRaster(fccs.raster, to = wind.map, method='ngb') #I chose nearest neighbor for the fuels maps cause its a categorical variable
fuel.map <- fccs.raster

##Reading in uphill slope azimuth map
slope.azi.map <- raster("I:/LANDIS_code/2017FireExt/DEM/UphillSlopeAzimuth.img")
uphill.azi.map <- projectRaster(slope.azi.map, to = fuel.map, method='bilinear')

SNslope.map <-  raster("I:/LANDIS_code/2017FireExt/DEM/SN_slope.img")
slope.map <- projectRaster(SNslope.map, to = fuel.map, method='bilinear')

wind.map <- projectRaster(wind.map, to = fuel.map, method='ngb')

##Stacking all rasters as a test
climate.stack <- stack(wind.map, fuel.map, uphill.azi.map, slope.map)

#FWIs from SCRAPPLE
fwi.dat <- read.csv("I:/LANDIS_code/Testers/SCRAPPLE/FWI_getter/Climate-future-input-log.csv")


#Setting up names of fire perimeter shapefiles
years <- 2000:2016

perimeter.map.names <- list.files(fire.dir, full.names = TRUE)
perimeter.map.names <- grep(".shp", perimeter.map.names, value = TRUE) 
perimeter.map.names <- grep(".xml", perimeter.map.names, value=TRUE, invert=TRUE)

perimeter.map.names.1 <- perimeter.map.names[2:7]

## Setting up vectors to loop through in order to standardize fire name and fire date column names. These are the ones from the fire perimeter maps

firenames.cols <- c("firename", "firename", "event_name", "firename", "firename", "firename", "fire_name",
                    "fire_name", "fire_name", "fire_name", "fire_name", "fire_name", "fire_name", "fire_name",
                    "fire_name", "fire_name", "incidentna") ##This is a list of column names for fire names across all the years since they aren't consistent in GeoMAC

firedates.cols <- c("date_", "repdate", "c_date", "date_", "perim_date", "perim_date", "date_uploa", "date_uploa",
                    "date_", "date_", "date_", "date_", "date_", "date_", "date_", "date_", "datecurren")


################## PROCESSING EXTERNAL DATA SETS TO USE WITH EPA REGION AND CONSUME LOOK UP TABLES #####################
### THIS IS WHERE I TAKE CATEGORICAL VARIABLES FROM INPUT RASTERS AND SET UP LOOKUPS FOR REAL VALUES OF WIND SPEED, FUEL BIOMASS ETC ######

##Process FWI values produced by LANDIS climate library to prep for attaching to larger spread matrix
##Need to attach ecoregion info
fwi.dat.slim <- fwi.dat[,c(1,2,3,13)]
fwi.date.info <- with(fwi.dat.slim, paste(Year, Timestep))
fwi.dates <- strptime(fwi.date.info, "%Y %j") #COnverting from julian day to y-m-d
fwi.date.dat <- cbind(as.POSIXct(fwi.dates), fwi.dat.slim[,3:4]) #attaching 
colnames(fwi.date.dat) <- c("Date", "Ecoregion", "FWI")


##Attach LANDFIRE fuel loading info based on fuel type number
##I selected columns I thought would define fine fuels, but these can change if we need. Units are tons/acre
fccs.loading <- fccs.defs[,c("fuelbed_number", "understory_loading",  "litter_loading", "lichen_loading", "duff_upper_loading", "duff_lower_loading")] ##removed "shrubs_primary_loading", "shrubs_secondary_loading",
fccs.loading.total <- rowSums(fccs.loading[,2:length(fccs.loading)])
fccs.loading.total <- fccs.loading.total * 224.17 #convert from tons/acre to g/m-2
fccs.finefuels <- cbind(fccs.loading[,1], fccs.loading.total)
colnames(fccs.finefuels) <- c("fueltype", "finefuels_loading")

##Linking up successful spread days with actual wind speed from GeoDataPortal-created csv
wsv.dat <- cbind.data.frame(as.POSIXct(wsv.dat[,1]), wsv.dat[,2:6])
colnames(wsv.dat) <- c("date", 13, 4, 5, 80, 9)

###################### Loop through year to select all individual fires and fire days ###########################
########### Loop through individual fire days to extract successful and unsuccessful spread cells ###############

## Basic workflow is:
##Select an individual fire
##Remove repeat days 
##Remove last day and fires that lasted only one day 
##Create wind perimeter from appropriate wind day raster 
##Extract values of successful spread 

#Empty objects and column names and stuff used in loops

climate.day.mat <- NULL
#Extracting successful and unsuccessful spread variables
## This has to be done one year at a time now, since the resampling made each year's data humongous

#for(k in 1:length(perimeter.map.names)){ ##:length(perimeter.map.names)
  perimeter.map <-readOGR(perimeter.map.names[9]) # , proj4string = crs(fuel.map) reading in annual fire perimeter maps 
  perimeter.map <- spTransform(perimeter.map, crs(fuel.map))
  ##Need to rename attribute for firename for consistency across years
  names(perimeter.map)[names(perimeter.map) == firenames.cols[9]] <- "fire_name" ##renames attribute for fire name
  names(perimeter.map)[names(perimeter.map) == firedates.cols[9]] <- "date_" ##renames attribute for fire date
  
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
      fire.day.select.1 <- subset(fire.select, fire.select$date_== fire.days[j+1])#getting day 2 fire perimeter
      fire.day.select.1 <- fire.day.select.1[1,] #selecting the first fire perim from that date, in case there are multiples
      
      ##Getting wind direction. This is inferred from direction of spread, where aziumth of spread = wind direction
      ##This is getting an error now?! 1/30/2017
      fire.day.1.centroid <- gCentroid(fire.day.select.1) #Finding centroid for estimatign wind direction
      fire.day.centroid <- spTransform(fire.day.centroid, CRS("+proj=longlat +datum=WGS84")) ##Need to convert back to lat/long to get azimuth direction
      fire.day.1.centroid <- spTransform(fire.day.1.centroid, CRS("+proj=longlat +datum=WGS84")) ##Need to convert back to lat/long to get azimuth direction
      
      fire.azimuth <- bearingRhumb(fire.day.centroid, fire.day.1.centroid) #find the azimuth of fire spread by Rhumb bearing. This is used as a proxy assumed wind direction
      
      
      ##Getting area of spread by subtracting perimeter of day 1 from day 2
      perimeter.expansion <- perimeter(fire.day.select.1) - perimeter(fire.day.select)
      area.expansion <- fire.day.select.1$acres - fire.day.select$acres
      
      #print(fire.day.select.1$date_)
      
      #Creating vector of fire dates. It doesnt work when I bind them below for some reason 
      date.char <- as.POSIXct(as.character(fire.day.select$date_)) #, format = "%Y-%m-%d", tz="" this stopped working? need to strip PDT then
      
      ##Extracting climate and fuels variables from raster stack.
      #This will compile a data frame of every cell that fire perimeters touch and whether 
      day1 <- extract(climate.stack, fire.day.select, cellnumbers = TRUE, df=TRUE, method="simple")
      day2 <- extract(climate.stack, fire.day.select.1, cellnumbers = TRUE, df=TRUE, method="simple")
      spread.success.inverse <- as.numeric(day2$cell %in% day1$cell) #Matching cell numbers to find spread and nonspread cells
      spread.success <- ifelse(spread.success.inverse == 0, 1,
                        ifelse(spread.success.inverse==1, 0, 0)) #reversing codes for spread. spread now == 1, nonspread == 0 
      #print(spread.success) #QAQC
      climate.day.df <- cbind(fire.day.select$fire_name, date.char, day2[,2:6], fire.azimuth,
                              area.expansion, spread.success) #Putting everything into a big dataframe with all climate variables
      
      #dates <- climate.day.df[,2]
      #dates.full <- c(dates.full, dates)
      
      climate.day.mat <- rbind.data.frame(climate.day.mat, climate.day.df) #binding individual days to everyother day

    }
  }
#}

rm(i) #removing loopers
rm(j)
rm(k)

#Removing rows with NA values
climate.day.complete <- climate.day.mat[complete.cases(climate.day.mat),]

##Attaching a unique ID to each row in case we need it later
climate.day.complete <- cbind(1:nrow(climate.day.complete), climate.day.complete)

##Renaming columns
colnames(climate.day.complete) <- c("ID", "fire_name", "date", "cell", "wind_region", "fuel_number", "uphill_azi", "slope",
                               "fire_azimuth", "expansion", "spread")

### ATTACHING REAL VALUES TO WIND SPEED, FWI, AND FUEL BIOMASS BASED ON EXTERNAL DATA SOURCES #######
##Loop that attaches actual wind speed, FWI, and fuel biomass to successul spread cell days. Lookup tables are defined above first loop

climate.fuel.df <- matrix(nrow=nrow(climate.day.complete), ncol = 3)

for(i in 1:nrow(climate.day.complete)){
  fire.cell.select <- climate.day.complete[i,]
  #print(fire.cell.select$date)
  
  #extracting real wind speed velocity value for this day, using EPA region as reference number
  wsv.day <- wsv.dat[grep(fire.cell.select$date, wsv.dat[,1]), match(fire.cell.select$wind_region, colnames(wsv.dat))]
  
  #makeshift progress bar
  print((i/nrow(climate.day.complete))*100)
  
  #extracting FWI value for they day based on 
  fwi.days <- fwi.date.dat[grep(fire.cell.select$date, fwi.date.dat[,1]),] #matching dates to climate library output
  fwi.day <- fwi.days[match(fire.cell.select$wind_region, fwi.days[,2]),3] #extracting fwi from correct ecoregion
  
  #extracting fuel biomass from FCCS class look up table
  fuel.day <- fccs.finefuels[match(fire.cell.select$fuel_number, fccs.finefuels[,1]),2]
  
  day.variables <- c(wsv.day, fwi.day, fuel.day)
  
  #Binding all variables to original dataframe of climate and fuel raster extractions
  climate.fuel.df[i,] <- day.variables 
  
}
rm(i)

colnames(climate.fuel.df) <- c("Windspeed", "FWI", "finefuels")

climate.vars.df <- cbind.data.frame(climate.day.complete, climate.fuel.df)

spread.vars.complete <- climate.vars.df[complete.cases(climate.vars.df),]  ##This is really screwing stuff up. Something has a bunch of NAs and this is getting rid of alot of rows
colnames(spread.vars.complete) <- c(colnames(climate.day.complete), colnames(climate.fuel.df))

##Exporting a csv of the fire spread variable database. This will get updated fairly frequently but I can overwrite as necessary
##it'll just save time when trying to do function fitting to have an external file to play with

write.csv(spread.vars.complete, file = "I:/LANDIS_code/2017FireExt/Spread/FireSpreadVars_Prelim08_short.csv")


##########################################################################################################################################
################## FITTING FUNCTION OF CLIMATE AND FUELS VARIABLES TO DOSE RESPONSE, IE SPREAD VS NONSPREAD ##############################
#########################################################################################################################################

#spread.vars.complete <- read.csv("I:/LANDIS_code/2017FireExt/Spread/FireSpreadVars_Prelim03_short.csv", row.names = NULL)

#Short loop to read in individual year csvs
list.years <- list.files("I:/LANDIS_code/2017FireExt/Spread/", full.names = TRUE)

spread.vars.complete <- NULL
for (i in list.years){
  year.select <- read.csv(i)
  spread.vars.complete <- rbind(spread.vars.complete, year.select)
}

## ELIMINATE NEGAITIVE SPREAD DAYS ###
spread.vars.complete <- spread.vars.complete[(spread.vars.complete$expansion) > 0,]

###### SELECT VARIABLES USED FOR FUNCTION FITTING ####
spread.vars.short <- spread.vars.complete[,c("spread", "FWI", "finefuels", "Windspeed", "uphill_azi", 
                                             "fire_azimuth", "slope", "expansion")]


##Relativize fine fuels
# Using max (nonoutlier) fuel loading from CONSUME fuels database (15287.61) as 1.0
rel.finefuels <- spread.vars.short$finefuels/15287.61
spread.vars.short <- cbind.data.frame(spread.vars.short[,1:2], rel.finefuels, spread.vars.short[,4:8])


##Converting wind speed to effective wind speed based on slope and uphill slope azimuth
##These conversions are based off Nelson 2002

# Windspeed <- 5
U.b <- 5 #mystery value of 'combustion bouyancy'. This changes based on fire severity. Check out GItHub and how its coded in SCRAPPLE itself
# relative.wd <-  wind.direction - upslope.azi
relative.wd <- spread.vars.short$fire_azimuth - spread.vars.short$uphill_azi


effective.wsv <- U.b * ((spread.vars.short$Windspeed/U.b) ^ 2 + 2*(spread.vars.short$Windspeed/U.b) *  
                sin(spread.vars.short$slope) * cos(relative.wd) + (sin(spread.vars.short$slope)^2)^0.5)

spread.vars.short <- cbind(spread.vars.short, effective.wsv)

###### FUNCTION TO FIT AREA EXPANSION AS A FUNCTION OF WSV AND FWI ######

spread.vars.success <- as.data.frame(spread.vars.short[spread.vars.short$spread ==1,])


plot(spread.vars.success$effective.wsv, (spread.vars.success$expansion/2.46), xlab="Average daily wind speed velocity (m/s)", ylab="Area expansion (ha)",
     main = "Daily wind speed velocity and fire area spread")

expansion.lm <- glm((expansion/2.46) ~ FWI + effective.wsv, data = spread.vars.success)
summary(expansion.lm)

train.row <- sample(x=1:nrow(spread.vars.short), size = nrow(spread.vars.short)*.80)
spread.train <- spread.vars.short[train.row,]
spread.test <- spread.vars.short[-train.row,]

##Sensitivity testing of spread parameters. Fiited params
exp.test <- expansion.lm$coefficients[1] + expansion.lm$coefficients[2]*spread.train$FWI + 
  expansion.lm$coefficients[3]*spread.test$effective.wsv
hist(exp.test, main ="Predicted expansion")

##Tester
exp.test <- 3000 + -39*spread.train$FWI + 
  -2.5*spread.test$effective.wsv
hist(exp.test, main ="Predicted expansion")


#Augmented zero-inflated POisson distribution

p.mod <- zeroinfl(spread ~ effective.wsv + FWI + rel.finefuels, 
                  data = spread.train, link = "logit", dist="poisson")

summary(p.mod)

#Linear regression
l.mod <- glm(spread ~  FWI + rel.finefuels + effective.wsv, 
             data = spread.train)
summary(l.mod)

##trying out the anneal function
##creating model function
spread.func <- function(beta){
  1 / (1 + e ^ (beta[1] + (beta[2] * spread.test$FWI) +
    (beta[3]*spread.test$rel.finefuels) + (beta[4]*spread.test$effective.wsv)))

}

## optim()
## functions lifted from Stack Overflow help 
## https://stackoverflow.com/questions/44712419/estimate-a-probit-regression-model-with-optim
## Need to incorporate BEV CURVE


spread.short <- as.matrix(spread.test[,c(2,3,9)])
spread.response <- spread.test[,1]

# requires model matrix `spread.test` and binary response `spread.response`
probit.nll.complex <- function (beta) {
  ##Using Bev curve
  p <- 1 / (1 + e ^ (beta[1] + (beta[2] * spread.test$FWI) +
                  (beta[3]*spread.test$rel.finefuels) + (beta[4]*spread.test$effective.wsv)))
  
  # negative log-likelihood - THIS CAN STAY I THINK?
  -sum((1 - spread.response) * log(1 - p) + spread.response * log(p))
}


# requires model matrix `X` and binary response `Y`
probit.gr.complex <- function (beta) {
  # linear predictor
  # eta <- spread.short %*% beta
  # # probability
  # p <- pnorm(eta)
  # 
  # #bev curve
  p <- 1 / (1 + e ^ (beta[1] + (beta[2] * spread.test$FWI) +
                       (beta[3]*spread.test$rel.finefuels) + (beta[4]*spread.test$effective.wsv)))
  
  
  # chain rule
  #It doesn't know what ETA is
  u <- dnorm(eta) * (spread.response - p) / (p * (1 - p))
  # gradient
  -crossprod(spread.short, u)
}

set.seed(0)
# # model matrix
# X <- cbind(1, matrix(runif(300, -2, 1), 100))
# # coefficients
b <- runif(4) 
# # response
# Y <- rbinom(100, 1, pnorm(X %*% b))

# `glm` estimate
GLM <- glm(spread.response ~ spread.short - 1, family = binomial(link = "probit"))

# our own estimation via `optim`
# I am using `b` as initial parameter values
fit <- optim(b, probit.nll.complex,  hessian = TRUE) #,method = "L-BFGS-B", gr = probit.gr.complex


