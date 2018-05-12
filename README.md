# Microservices with .Net Core, Docker and Azure Service Fabric

## Prerequisites and Installation Requirements

1. Install [Docker for Windows](https://docs.docker.com/docker-for-windows/install/).
2. Install [.NET Core SDK](https://www.microsoft.com/net/download/windows)
3. Install [Visual Studio 2017](https://www.visualstudio.com/downloads/) 15.7 or later.
4. Share drives in Docker settings, in order to deploy and debug with Visual Studio 2017 (See the below image)
5. Clone this Repo
6. Set `docker-compose` project as startup project.
7. Press F5 and that's it!

![](https://github.com/vany0114/vany0114.github.io/blob/master/images/docker_settings_shared_drives.png)

> Note: The first time you hit F5 it'll take a few minutes, because in addition to compile the solution, it needs to pull/download the base images (SQL for Linux Docker, ASPNET, MongoDb and RabbitMQ images) and register them in the local image repo of your PC. The next time you hit F5 it'll be much faster.

### Tuning Docker for better performance

It is important to set Docker up properly with enough memory RAM and CPU assigned to it in order to improve the performance, or you will get errors when starting the containers with VS 2017 or "docker-compose up". Once Docker for Windows is installed in your machine, enter into its Settings and the Advanced menu option so you are able to adjust it to the minimum amount of memory and CPU (Memory: Around 4096MB and CPU:3) as shown in the image.

![](https://github.com/vany0114/vany0114.github.io/blob/master/images/docker_settings.png)

### Demo Screenshots
#### Website
![](https://github.com/vany0114/vany0114.github.io/blob/master/images/duber-in-action.gif)
#### Trip API
![](https://github.com/vany0114/vany0114.github.io/blob/master/images/duber-trip-api.png)
#### Invoice API
![](https://github.com/vany0114/vany0114.github.io/blob/master/images/duber-invoice-api.png)

Visit my blog <http://elvanydev.com/Microservices-part1/> to view all the posts and to know all the details about this project.
