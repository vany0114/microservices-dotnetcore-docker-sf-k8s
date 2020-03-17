# configmap
kubectl delete cm env-config

# external system
kubectl delete deploy payment
kubectl delete svc payment

# invoice
kubectl delete deploy invoice
kubectl delete ing invoice
kubectl delete svc invoice

# trip
kubectl delete deploy trip
kubectl delete hpa trip
kubectl delete ing trip
kubectl delete svc trip

# website
kubectl delete deploy frontend
kubectl delete hpa frontend
kubectl delete ing frontend
kubectl delete svc frontend

#notificatiosn
kubectl delete deploy notifications
kubectl delete hpa notifications
kubectl delete ing notifications
kubectl delete svc notifications