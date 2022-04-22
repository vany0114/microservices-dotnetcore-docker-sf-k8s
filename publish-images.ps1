# only for testing locally purposes, you should build and publish your images from a CI pipeline.

docker tag duber/trip.api:latest vany0114/duber.trip.api:latest
docker push vany0114/duber.trip.api:latest

docker tag duber/invoice.api:latest vany0114/duber.invoice.api:latest
docker push vany0114/duber.invoice.api:latest

docker tag duber/website:latest vany0114/duber.website:latest
docker push vany0114/duber.website:latest

docker tag externalsystem/paymentservice:latest vany0114/externalsystem.paymentservice:latest
docker push vany0114/externalsystem.paymentservice:latest

docker tag duber/trip.notifications:latest vany0114/duber.trip.notifications:latest
docker push vany0114/duber.trip.notifications:latest