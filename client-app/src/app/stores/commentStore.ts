import { HubConnection, HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { ChatComment } from "../models/comment";
import { makeAutoObservable, runInAction } from "mobx";
import { store } from "./store";

export default class CommentStore {
    comments: ChatComment[] = [];
    hubConnection: HubConnection | null = null;

    constructor() {
        makeAutoObservable(this);
    }

    createHubConnection = (activityId: string) => {
        if (store.activityStore.selectedActivity) {
            this.hubConnection = new HubConnectionBuilder()
                .withUrl('http://localhost:5000/chat?activityId=' + activityId, {
                accessTokenFactory: () => store.userStore.user?.token!
            })
                .withAutomaticReconnect()
                .configureLogging(LogLevel.Information)
                .build();

            this.hubConnection.start().catch(error => console.log('Error establishing connection: ', error));

            this.hubConnection.on('LoadComments', (comments: ChatComment[]) => { //see ChatHub.cs > OnConnectedAsync
                runInAction(() => {
                    comments.forEach(comment =>{ //memformat createdAt dari string menjadi object Date di JS
                        comment.createdAt = new Date(comment.createdAt +'Z'); // plus Z: any comments we receive and the load comments method We're getting these directly from our database.
                    })
                    this.comments = comments
                });
            })

            this.hubConnection.on('ReceiveComment', (comment: ChatComment) => { //see ChatHub.cs > SendCommand
                comment.createdAt = new Date(comment.createdAt);
                runInAction(() => this.comments.unshift(comment)); //push (at the end of array)=> unshift (put the comment at the start of the array)
            })
        }
    }

    stopHubConnection = () => {
        this.hubConnection?.stop().catch(error => console.log('Error stopping connection: ', error));
    }

    clearComments = () => {
        this.comments = [];
        this.stopHubConnection();
    }

    addComment = async (values: any)=> {
        // console.log('addComment here');
        values.activityId = store.activityStore.selectedActivity?.id;
        try {
            await this.hubConnection?.invoke('SendComment', values) //cocokin dengan nama method di API > SignalR > ChatHub.cs (MapHub dari endpoint 'chat')
        } catch (error) {
            console.log(error);
        }
    }
}