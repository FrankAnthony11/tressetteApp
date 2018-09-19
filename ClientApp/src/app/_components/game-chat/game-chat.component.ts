import { HubService } from './../../_services/hub.service';
import { Component, OnInit } from '@angular/core';
import { ChatMessage } from '../../_models/chatMessage';
import { User } from '../../_models/user';

@Component({
  selector: 'app-game-chat',
  templateUrl: './game-chat.component.html',
  styleUrls: ['./game-chat.component.css']
})
export class GameChatComponent implements OnInit {


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
    this._hubService.AddNewMessageToGameChat(this.newMessage);
    this.newMessage = '';
  }

}
