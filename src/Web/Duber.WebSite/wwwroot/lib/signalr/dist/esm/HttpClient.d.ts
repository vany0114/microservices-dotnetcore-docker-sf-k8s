import { AbortSignal } from "./AbortController";
import { ILogger } from "./ILogger";
export interface HttpRequest {
    method?: string;
    url?: string;
    content?: string | ArrayBuffer;
    headers?: {
        [key: string]: string;
    };
    responseType?: XMLHttpRequestResponseType;
    abortSignal?: AbortSignal;
    timeout?: number;
}
export declare class HttpResponse {
    readonly statusCode: number;
    readonly statusText: string;
    readonly content: string | ArrayBuffer;
    constructor(statusCode: number, statusText: string, content: string);
    constructor(statusCode: number, statusText: string, content: ArrayBuffer);
}
export declare abstract class HttpClient {
    get(url: string): Promise<HttpResponse>;
    get(url: string, options: HttpRequest): Promise<HttpResponse>;
    post(url: string): Promise<HttpResponse>;
    post(url: string, options: HttpRequest): Promise<HttpResponse>;
    delete(url: string): Promise<HttpResponse>;
    delete(url: string, options: HttpRequest): Promise<HttpResponse>;
    abstract send(request: HttpRequest): Promise<HttpResponse>;
}
export declare class DefaultHttpClient extends HttpClient {
    private readonly logger;
    constructor(logger: ILogger);
    send(request: HttpRequest): Promise<HttpResponse>;
}
