# this sets us up with the base image for any subsequent instructions that follow and we give this stage of the build
# a name
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build 
# this is a directory inside docker, not a directory inside our file system
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers. Use of the same COPY command for every dockerfile in the
# project to take advantage of docker caching

# copy our solution file; we're gonna be working in the roots of our solution, so we're just going to specify Carsite.sln
# and we're gonna copy this into a solution file of exactly the same name
# so this is gonna take it from our development machine's file system and copying this file into our working directory (/src)
# inside docker (first - our file system, second - docker's file system)
COPY Carsite.sln Carsite.sln
COPY src/AuctionService/AuctionService.csproj src/AuctionService/AuctionService.csproj
COPY src/SearchService/SearchService.csproj src/SearchService/SearchService.csproj
COPY src/GatewayService/GatewayService.csproj src/GatewayService/GatewayService.csproj
COPY src/Contracts/Contracts.csproj src/Contracts/Contracts.csproj
COPY src/IdentityService/IdentityService.csproj src/IdentityService/IdentityService.csproj

# Restore Package Dependencies - because we're in the context and using the dotnet sdk, we have access to the dotnet CLI
# this is gonna take a look at our solution file and all the csproj files and it's gonna run the dotnet restore command
# on that
RUN dotnet restore Carsite.sln

# Copy the app folders over
COPY src/AuctionService src/AuctionService
# we also need to do it for the contracts because AuctionService has dependency on the Contracts project as well
COPY src/Contracts src/Contracts

# Now, once we've copied those files over into our Docker directory, the source directory here we're working in, we can
# then publish what we have inside here
# in /app/src we're gonna have a folder named AuctionService because that is what we copied AuctionService.csproj into
WORKDIR /app/src/AuctionService
# we're using the publish command to publish our AuctionService application, and the publish files are gonna be stored
# in this out directory.
RUN dotnet publish -c Release -o /app/src/out

# now, at this stage we want to get a runtime image and we don't need to use the SDK, which is quite big for that, we 
# can actually use the .net runtime directly once we've got to this stage and we've got a published version of our AuctionService

# Build Runtime Image
# this gives us access to the much smaller dotnet runtime than we were using up here (dotnet/sdk)
FROM mcr.microsoft.com/dotnet/aspnet:7.0
# now that we've got the base image as just a dotnet runtime, we can now switch our directory back into /app
WORKDIR /app
# we take all of that content from build and put into the out directory of our current working directory,
# so we're gonna have /app/src/out and all of the published files inside there
COPY --from=build /app/src/out .
ENTRYPOINT ["dotnet", "AuctionService.dll"]