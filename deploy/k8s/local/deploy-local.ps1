# k8s dashboard
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0-beta8/aio/deploy/recommended.yaml
kubectl apply -f dashboard-adminuser.yaml

# nginx-ingress
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/master/deploy/static/mandatory.yaml
kubectl apply -f nginx-ingress\custom-service.yaml

# configmap
kubectl apply -f env-config.yaml

# mongo
kubectl apply -f mongo\mongo-admin.yaml
kubectl apply -f mongo\mongo-deployment.yaml
kubectl apply -f mongo\mongo-service.yaml

# redis
kubectl apply -f redis\redis-deployment.yaml
kubectl apply -f redis\redis-service.yaml

# rabbit
kubectl apply -f rabbit\rabbit-admin.yaml
kubectl apply -f rabbit\rabbit-deployment.yaml
kubectl apply -f rabbit\rabbit-service.yaml

# sql-server
kubectl apply -f sql-server\sql-admin.yaml
kubectl apply -f sql-server\sql-deployment.yaml
kubectl apply -f sql-server\sql-service.yaml

# external system
kubectl apply -f external-system\payment-deployment.yaml
kubectl apply -f external-system\payment-service.yaml

# invoice
kubectl apply -f invoice\invoice-deployment.yaml
kubectl apply -f invoice\invoice-ingress.yaml
kubectl apply -f invoice\invoice-service.yaml

# trip
kubectl apply -f trip\trip-deployment.yaml
kubectl apply -f trip\trip-hpa.yaml
kubectl apply -f trip\trip-ingress.yaml
kubectl apply -f trip\trip-service.yaml

# website
kubectl apply -f website\website-deployment.yaml
kubectl apply -f website\website-hpa.yaml
kubectl apply -f website\website-ingress.yaml
kubectl apply -f website\website-service.yaml

#notificatiosn
kubectl apply -f notifications\notifications-deployment.yaml
kubectl apply -f notifications\notifications-hpa.yaml
kubectl apply -f notifications\notifications-ingress.yaml
kubectl apply -f notifications\notifications-service.yaml