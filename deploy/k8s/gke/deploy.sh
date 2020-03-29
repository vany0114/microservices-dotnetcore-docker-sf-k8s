# configmap
kubectl apply -f env-config.yaml

# common ingress
kubectl apply -f default-ingress.yaml

# invoice
kubectl apply -f invoice\invoice-deployment.yaml
kubectl apply -f invoice\invoice-service.yaml
kubectl apply -f invoice\invoice-hpa.yaml

# trip
kubectl apply -f trip\trip-deployment.yaml
kubectl apply -f trip\trip-hpa.yaml
kubectl apply -f trip\trip-service.yaml

# website
kubectl apply -f website\website-deployment.yaml
kubectl apply -f website\website-hpa.yaml
kubectl apply -f website\website-service.yaml

#notificatiosn
kubectl apply -f notifications\notifications-deployment.yaml
kubectl apply -f notifications\notifications-hpa.yaml
kubectl apply -f notifications\notifications-ingress.yaml
kubectl apply -f notifications\notifications-service.yaml