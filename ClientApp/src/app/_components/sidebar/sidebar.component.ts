import { Component, OnInit } from "@angular/core";
import { User } from "app/_models/user";
import { WaitingRoom } from "app/_models/waitingRoom";
import { Game } from "app/_models/game";
import { HubService } from "app/_services/hub.service";

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  users: User[];
  waitingRooms: WaitingRoom[];
  runningGames:Game[];

  constructor(private _hubService: HubService) {}

  ngOnInit(): void {
    this._hubService.Users.subscribe((users: User[]) => {
      this.users = users;
    });

    this._hubService.WaitingRooms.subscribe((waitingRooms: WaitingRoom[]) => {
      this.waitingRooms = waitingRooms;
    });

    this._hubService.AllRunningGames.subscribe((runningGames: Game[]) => {
      this.runningGames = runningGames;
    });
  }
  
  joinGameAsSpectator(runningGame:Game){
    this._hubService.JoinGameAsPlayerOrSpectator(runningGame.id);

  }

  joinWaitingRoom(waitingRoom: WaitingRoom) {
    let password = '';
    if (waitingRoom.password) {
      password = prompt('Input password for this game');
      if (password == null) return;
    }
    this._hubService.JoinWaitingRoom(waitingRoom.id, password);
  }

  buzzPlayer(user:User){
    this._hubService.AddNewMessageToAllChat(`/buzz ${user.name}`)
  }


}
