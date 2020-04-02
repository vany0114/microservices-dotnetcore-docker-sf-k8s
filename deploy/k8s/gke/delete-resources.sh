# configmap
kubectl delete cm env-config

# common ingress
kubectl delete ing default-ingress

# invoice
kubectl delete deploy invoice
kubectl delete svc invoice
kubectl delete hpa invoice

# trip
kubectl delete deploy trip
kubectl delete hpa trip
kubectl delete svc trip

# website
kubectl delete deploy frontend
kubectl delete hpa frontend
kubectl delete svc frontend

#notificatiosn
kubectl delete deploy notifications
kubectl delete hpa notifications
kubectl delete ing notifications
kubectl delete svc notifications