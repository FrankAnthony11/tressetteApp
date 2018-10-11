import { Component, OnInit } from "@angular/core";
import { HubService } from "app/_services/hub.service";


@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  constructor(private _hubService: HubService) {}

  ngOnInit(): void {
    this._hubService.LeaveWaitingRoom();
  }

  createNewWaitingRoom(playUntilPoints: number, expectedNumberOfPlayers: number) {
    this._hubService.CreateWaitingRoom(playUntilPoints, expectedNumberOfPlayers);
  }


}
