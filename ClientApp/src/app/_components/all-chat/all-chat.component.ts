import { Component, OnInit } from "@angular/core";
import { ChatMessage } from "app/_models/chatMessage";
import { User } from "app/_models/user";
import { HubService } from "app/_services/hub.service";
import { TypeOfMessage } from "app/_models/enums";

@Component({
  selector: 'app-all-chat',
  templateUrl: './all-chat.component.html',
  styleUrls: ['./all-chat.component.css']
})
export class AllChatComponent implements OnInit {

  messages: ChatMessage[];
  currentUser: User;
  newMessage = '';

  constructor(private _hubService: HubService) {}
  ngOnInit(): void {
    this._hubService.AllChatMessages.subscribe(nn => {
      this.messages = nn;
    });
    this._hubService.CurrentUser.subscribe(user => {
      this.currentUser = user;
    });
  }

  sendMessageAllChat() {
    if (!this.newMessage) return;
    this._hubService.SendMessageToAllChat(this.newMessage);
    this.newMessage = '';
  }

  getChatMessageClass(message: ChatMessage) {
    if (message.typeOfMessage == TypeOfMessage.server) {
      return 'server-chat-message';
    }
  }


}
