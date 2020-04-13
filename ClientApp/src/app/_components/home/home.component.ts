import { GameSetupComponent } from './../_modals/game-setup/game-setup.component';
import { Component, OnInit } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  ngOnInit(): void { }
  constructor(private _modalService: NgbModal) { }

  createGame() {
    var userSettingsModal = this._modalService.open(GameSetupComponent, { backdrop: 'static', keyboard: false });
    return userSettingsModal;
  }

  rename() {
    let name = prompt('Input your name');
    if (!name) return;
    localStorage.setItem('name', name);
    window.location.reload();
  }
}
