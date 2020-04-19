# Local deployment using Kubernetes

## Prerequisites and Installation Requirements

1. Install Docker for [Windows](https://docs.docker.com/docker-for-windows/install/)/[Mac](https://docs.docker.com/docker-for-mac/install/).
2. Make sure to check the opton *Enable Kubernetes* on your Docker settings.
![](https://github.com/vany0114/vany0114.github.io/blob/master/images/docker-desktop-k8s.png)
3. Run `deploy-local.ps1` script (located at `\deploy\k8s\local`) to deploy the solution on your local Kubernetes cluster.
4. Add `duber.local.com` and `trip.notifications.local.com` domains to your `hosts` file. Those are the hosts using by our Ingress in order to expose the Frontend and the SignalR Trip Notifications service respectively.
```
127.0.0.1 duber.local.com
127.0.0.1 trip.notifications.local.com
```
> Optionally, if you want to expose the API's you have to add `invoice.local.com` and `trip.local.com` domains too.
```
127.0.0.1 invoice.local.com
127.0.0.1 trip.local.com
```
6. Go to http://duber.local.com:81/ and you'll see the application up and working!

## Admin tools
To be able to access to our SQL or Mongo databases through an IDE, or to RabbitMQ' dashboard, we exposed a port through a `Node Port`:
* SQL Server: `31433`, connection example: `tcp:127.0.0.1,31433`
* Mongo: `31434`, connection example: `mongodb://0.0.0.0:31434/`
* RabbitMQ: `31672`, connection example: `http://localhost:31672/#/`

## Architecture
![](https://github.com/vany0114/vany0114.github.io/blob/master/images/Duber_Kubernetes_Local_Environment_Architecture.png)
