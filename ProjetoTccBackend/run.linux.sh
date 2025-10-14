#!/bin/sh

echo Select the project Environment:
echo "1 - Development"
echo "2 - Staging"
echo "3 - Production"
read -r option


if [ "$option" = "1" ]
then
    export ASPNETCORE_ENVIRONMENT=Development
elif [ "$option" = "2" ]
then
    export ASPNETCORE_ENVIRONMENT=Staging
elif [ "$option" = "3" ]
then
    export ASPNETCORE_ENVIRONMENT=Production
else
    echo "Invalid option"
    exit 1
fi

ASPNETCORE_PROFILE={""}

if [ "$ASPNETCORE_ENVIRONMENT" = "Development" ]
then
    ASPNETCORE_PROFILE="Debug"
elif [ "$ASPNETCORE_ENVIRONMENT" = "Staging" ]
then
    ASPNETCORE_PROFILE="Production"
elif [ "$ASPNETCORE_ENVIRONMENT" = "Production" ]
then
    ASPNETCORE_PROFILE="Production"
fi

exec dotnet run -lp https --os linux --no-self-contained -c "$ASPNETCORE_PROFILE"