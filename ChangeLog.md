## 2.1.0
**Support to deploy on Cloud K8s cluter:**
* Adjust manifests to deploy in a cloud cluster as a cloud native solution
* Deployed, ran and tested on Google Kubernetes Engine (GKE)

## 2.0.4
**Health checks implementation & ConfigMap set up:**
* Add health checks to all microservices
* Add health checks UI add-on to frontend
* Upgrade EF Core to `3.0.0`
* Organize better the `Startup` files
* Set up k8s probes
* Move env variables to a common `ConfigMap`

## 2.0.3
**Event bus handlers idempotency:**
* Add *Duber.Infrastructure.EventBus.Idempotency* project to handle idempotency at integration events level.
* Make *TripFinishedIntegrationEvent* idempotent in order to avoid it can be paid more than once due to concurrency, retries, etc.

## 2.0.2
**Notifications service:**
* Create an independent service to manage the notifications in order to decouple it from the frontend and to allow a better scaling out for both, frontend and notifications service.
* Add Redis to the cluster in order for *SignalR* to work properly into the cluster.
* Disable *Sticky Sessions* in frontend's Ingress to allow a better load balancing since the notifications don't depend on the frontend anymore. The notification's Ingress is the one that has *Sticky Sessions* to manage the *SignalR* connections.

## 2.0.1
**Kubernetes support:**
* Enable the solution to being deployed on a local cluster
* Use an Nginx Ingress Controller to expose frontend (Trip and Invoice services optional if you want to expose the API's)
* Set up Nginx LB to use Sticky sessions in order for SignalR to work properly.

**Frontend client dependencies:**
* Use Libman to manage the client dependencies. 
* Delete static *SignalR* client dependencies, using Libman now.

**General enhancement:**
* Refactor SignalR messaging in order to send messages only tho the connected client rather than all clients.
* Refactor RabbitMQ client in order to use named Queues and full support to async handlers.

## 2.0.0
* Upgrades to .Net Core 3.1
* Makes Trip and Invoice API's RESTful
* Refactors ***Duber.Infrastructure.Resilience.Sql*** project to follow the pattern proposed [here](https://github.com/vany0114/resilience-strategy-with-polly)
* Gets rid of Restsharp dependency in order to use HttpClient.
* Upgrade Kledex package (formerly Weapsy.CQRS/OpenCQRS)
