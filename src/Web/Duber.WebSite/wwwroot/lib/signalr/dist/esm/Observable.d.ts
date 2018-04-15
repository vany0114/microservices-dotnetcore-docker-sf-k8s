export interface Observer<T> {
    closed?: boolean;
    next: (value: T) => void;
    error?: (err: any) => void;
    complete?: () => void;
}
export declare class Subscription<T> {
    private subject;
    private observer;
    constructor(subject: Subject<T>, observer: Observer<T>);
    dispose(): void;
}
export interface Observable<T> {
    subscribe(observer: Observer<T>): Subscription<T>;
}
export declare class Subject<T> implements Observable<T> {
    observers: Array<Observer<T>>;
    cancelCallback: () => Promise<void>;
    constructor(cancelCallback: () => Promise<void>);
    next(item: T): void;
    error(err: any): void;
    complete(): void;
    subscribe(observer: Observer<T>): Subscription<T>;
}
