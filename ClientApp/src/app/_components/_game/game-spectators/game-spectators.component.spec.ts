/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { GameSpectatorsComponent } from './game-spectators.component';

describe('GameSpectatorsComponent', () => {
  let component: GameSpectatorsComponent;
  let fixture: ComponentFixture<GameSpectatorsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GameSpectatorsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GameSpectatorsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
