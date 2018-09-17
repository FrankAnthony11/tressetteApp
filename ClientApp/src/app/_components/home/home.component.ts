import { Component } from '@angular/core';
import { WaitingRoom } from '../../_models/waitingRoom';
import { HubService } from '../../_services/hub.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent {
  constructor(private _hubService: HubService) {}

  createNewWaitingRoom(playUntilPoints:number) {
    this._hubService.CreateWaitingRoom(playUntilPoints);    
  }
}
