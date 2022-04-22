param([String]$service="all")

if ($service -eq "all")
{
    docker-compose -f docker-compose.yml build
}
else
{
    docker-compose -f docker-compose.yml build $service
}