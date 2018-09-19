/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { GameChatComponent } from './game-chat.component';

describe('GameChatComponent', () => {
  let component: GameChatComponent;
  let fixture: ComponentFixture<GameChatComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GameChatComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GameChatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
