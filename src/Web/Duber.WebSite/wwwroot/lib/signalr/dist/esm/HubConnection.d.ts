import { ConnectionClosed } from "./Common";
import { IHttpConnectionOptions } from "./HttpConnection";
import { IConnection } from "./IConnection";
import { IHubProtocol } from "./IHubProtocol";
import { JsonHubProtocol } from "./JsonHubProtocol";
import { Observable } from "./Observable";
export { JsonHubProtocol };
export interface IHubConnectionOptions extends IHttpConnectionOptions {
    protocol?: IHubProtocol;
    timeoutInMilliseconds?: number;
}
export declare class HubConnection {
    private readonly connection;
    private readonly logger;
    private protocol;
    private handshakeProtocol;
    private callbacks;
    private methods;
    private id;
    private closedCallbacks;
    private timeoutHandle;
    private timeoutInMilliseconds;
    private receivedHandshakeResponse;
    constructor(url: string, options?: IHubConnectionOptions);
    constructor(connection: IConnection, options?: IHubConnectionOptions);
    private processIncomingData(data);
    private processHandshakeResponse(data);
    private configureTimeout();
    private serverTimeout();
    private invokeClientMethod(invocationMessage);
    private connectionClosed(error?);
    start(): Promise<void>;
    stop(): Promise<void>;
    stream<T>(methodName: string, ...args: any[]): Observable<T>;
    send(methodName: string, ...args: any[]): Promise<void>;
    invoke(methodName: string, ...args: any[]): Promise<any>;
    on(methodName: string, newMethod: (...args: any[]) => void): void;
    off(methodName: string, method?: (...args: any[]) => void): void;
    onclose(callback: ConnectionClosed): void;
    private cleanupTimeout();
    private createInvocation(methodName, args, nonblocking);
    private createStreamInvocation(methodName, args);
    private createCancelInvocation(id);
}
