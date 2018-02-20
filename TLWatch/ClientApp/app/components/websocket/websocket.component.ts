import { Component, Inject } from '@angular/core';
import { v4 as uuid } from 'uuid';

@Component({
    selector: 'websocket',
    templateUrl: './websocket.component.html',
    styleUrls: ['./websocket.component.less']
})
export class WebSocketComponent {
    socket: WebSocket | null = null;
    baseWSUrl: string;

    constructor(
        @Inject('BASE_URL') private readonly baseUrl: string
    ) {
        console.log("base", baseUrl);
        this.baseWSUrl = baseUrl.replace(/^http(s?)/, "ws$1");
    }

    connect() {
        if (this.socket) {
            return;
        }
        this.socket = new WebSocket(this.baseWSUrl + "ws/Chat");
        this.socket.addEventListener("open", event => {
            console.log("open", event);
        })
        this.socket.addEventListener("close", event => {
            console.log("close", event);
        })
        this.socket.addEventListener("error", event => {
            console.log("error", event);
        });
        this.socket.addEventListener("message", event => {
            let data = JSON.parse(event.data);
            console.log("message", data);
            let msg = new WebSocketMessageResponse(data.id, data.success, data.hasValue, data.value);
            this.messages.push(data);
        });
    }

    messages = new Array<WebSocketMessageResponse>();
    echoStr: string | null = null;
    echo() {
        if (!this.socket) {
            return;
        }
        let req = new WebSocketMessageRequest(uuid(), "echo", [this.echoStr]);
        console.log("send", req);
        this.socket.send(JSON.stringify(req));
    }
}

class WebSocketMessageRequest {
    constructor(
        public id: string,
        public action: string,
        public args: (Object | null)[]
    ) {
    }
}

class WebSocketMessageResponse {
    constructor(
        public id: string,
        public success: boolean,
        public hasValue: boolean,
        public value: Object | null
    ) {
    }
}
