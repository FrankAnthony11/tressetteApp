/* tslint:disable:no-unused-variable */
import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { DebugElement } from '@angular/core';

import { AllChatComponent } from './all-chat.component';

describe('AllChatComponent', () => {
  let component: AllChatComponent;
  let fixture: ComponentFixture<AllChatComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AllChatComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AllChatComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
