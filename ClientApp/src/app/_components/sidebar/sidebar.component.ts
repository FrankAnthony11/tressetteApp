import { Component, OnInit } from '@angular/core';
import { User } from 'app/_models/user';
import { Game } from 'app/_models/game';
import { HubService } from 'app/_services/hub.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {
  users: User[] = new Array<User>();
  runningGames: Game[] = new Array<Game>();

  constructor(private _hubService: HubService) {}

  ngOnInit(): void {
    this._hubService.Users.subscribe((users: User[]) => {
      this.users = users;
    });

    this._hubService.AllRunningGames.subscribe((runningGames: Game[]) => {
      this.runningGames = runningGames;
    });
  }

  joinGame(game: Game) {
    let password = '';
    
    if (game.gameSetup.isPasswordProtected) {
      password = prompt('Input password for this game');
      if (password == null) return;
    }
    this._hubService.JoinGame(game.gameSetup.id, password);
  }

  buzzPlayer(user: User) {
    this._hubService.SendMessageToAllChat(`/buzz ${user.name}`);
  }
}
