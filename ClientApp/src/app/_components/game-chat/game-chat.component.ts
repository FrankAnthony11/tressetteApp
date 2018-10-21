import { Component, OnInit } from "@angular/core";
import { ChatMessage } from "app/_models/chatMessage";
import { User } from "app/_models/user";
import { HubService } from "app/_services/hub.service";
import { TypeOfMessage } from "app/_models/enums";


@Component({
  selector: 'app-game-chat',
  templateUrl: './game-chat.component.html',
  styleUrls: ['./game-chat.component.css']
})
export class GameChatComponent implements OnInit {

  hideSpectatorsChat=false;

  messages: ChatMessage[];
  currentUser: User;
  newMessage = '';

  constructor(private _hubService: HubService) {}

  ngOnInit(): void {
    this._hubService.GameChatMessages.subscribe(nn => {
      this.messages = nn;
    });
    this._hubService.CurrentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  sendMessageToGameChat() {
    if (!this.newMessage) return;
    this._hubService.SendMessageToGameChat(this.newMessage);
    this.newMessage = '';
  }

  getChatMessageClass(message:ChatMessage){
    if(message.typeOfMessage==TypeOfMessage.server){
      return "server-chat-message"
    }
  }
  getChatMessageHidden(message:ChatMessage){
   return message.typeOfMessage==TypeOfMessage.spectators && this.hideSpectatorsChat;
  }

}
