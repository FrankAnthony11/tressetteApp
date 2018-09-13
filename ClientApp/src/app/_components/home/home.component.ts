import { Component } from '@angular/core';
import { HubService } from '../../_services/hub.service';
import { WaitingRoom } from '../../_models/waitingRoom';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {
  players: string[];
  waitingRooms: string[];

  constructor(private _hubService: HubService) {}
  
  ngOnInit(): void {
    this._hubService.Players.subscribe((players: string[]) => {
      players = players;
    });

    this._hubService.WaitingRooms.subscribe((waitingRooms:WaitingRoom[]) => {
      waitingRooms = waitingRooms;
    });
  }
}
