import { Component, OnInit } from '@angular/core';
import { HubService } from 'app/_services/hub.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {

  deckType: number;

  ngOnInit(): void {}
  constructor(private _hubService: HubService) {
    localStorage.setItem('typeOfDeck', "1");
  }

  createGame(playUntilPoints: number, expectedNumberOfPlayers: number) {
    this._hubService.CreateGame(playUntilPoints, expectedNumberOfPlayers, +localStorage.getItem("typeOfDeck"));
  }

  rename() {
    let name = prompt('Input your name');
    if (!name) return;
    localStorage.setItem('name', name);
    window.location.reload();
  }

  setDeckType(typeOfDeck: number) {
    localStorage.setItem('typeOfDeck', ""+typeOfDeck);
  }
}
