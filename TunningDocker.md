# Tuning Docker for better performance and Debugging

It is important to set Docker up properly with enough memory RAM and CPU assigned to it in order to improve the performance on your local/development environment, or you will get errors when starting the containers with VS 2019 or "docker-compose up". Once Docker is installed in your machine, enter into its Settings and the Advanced menu option so you are able to adjust it to the minimum amount of memory and CPU (Memory: Around 4096MB and CPU:3) as shown in the image.

![](https://github.com/vany0114/vany0114.github.io/blob/master/images/docker_settings.png)

Share drives in Docker settings, in order to deploy it as a Docker Compose application and also to debug with Visual Studio 2019 (See the below image)

![](https://github.com/vany0114/vany0114.github.io/blob/master/images/docker_settings_shared_drives.png)

> Note: The first time you hit F5 it'll take a few minutes, because in addition to compile the solution, it needs to pull/download the base images (SQL for Linux Docker, ASPNET, MongoDb and RabbitMQ images, etc) and register them in the local image repo of your PC. The next time you hit F5 it'll be much faster.