## This script contains the equatiosn SCRAPPLE is using to calculate number of ignitions and probability of spread
## It is to be used as a means of data exploration and testing expected behavior
## All predictor variables can be supplied as individual values or dataframes of multiple values

##Alec Kretchun, Portland State, 2017
## No support provided

###### Number of ignitions ~ FWI #################
### B0 and B1 are parameters derived from the Zero-inflated Poisson distribution (zeroinfl()), and correspond to SCRAPPLE input parameters
###   See Ignitions_FWI_function for further guidance ####
FWI <- 35
B0 <- -5
B1 <- 0.05
ign.test <- e^(B0 + (B1*FWI)) ##Number of fires/day as a function of FWI
ign.test


##### Probability of spread ##########
## Parameters B0, B1, B2, B3 correpsond to a generlized logistic function, and correspond to SCRAPPLE input parameters

FWI <- 40
EffectiveWindSpeed <- 7.2
FineFuels <- 2000

B0 <- -36
B1 <- 0.06
B2 <- .915
B3 <- .0126


e <- exp(1) #for creating e
b <- B0 + (B1*FWI) + (B2*EffectiveWindSpeed) + (B3*FineFuels)
z <- e^-b
p <- 1/(1 + z)
p
plot(p)


##Maximum area spread
FWI <- 50
EffectiveWindSpeed <- 7.2

B0 <- seq(-100, 0, .1)
B1 <- 2
B2 <- 2.5


spread <- B0 + (B1*FWI) + (B2*EffectiveWindSpeed)
plot(spread)
