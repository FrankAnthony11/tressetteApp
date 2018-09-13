/* tslint:disable:no-unused-variable */

import { TestBed, async, inject } from '@angular/core/testing';
import { HubService } from './hub.service';

describe('Service: Hub', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [HubService]
    });
  });

  it('should ...', inject([HubService], (service: HubService) => {
    expect(service).toBeTruthy();
  }));
});
