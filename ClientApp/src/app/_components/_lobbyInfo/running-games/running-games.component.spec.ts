/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { RunningGamesComponent } from './running-games.component';

describe('RunningGamesComponent', () => {
  let component: RunningGamesComponent;
  let fixture: ComponentFixture<RunningGamesComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RunningGamesComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RunningGamesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
