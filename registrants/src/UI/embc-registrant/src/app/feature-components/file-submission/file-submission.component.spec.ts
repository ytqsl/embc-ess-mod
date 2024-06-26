import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { FileSubmissionComponent } from './file-submission.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { MatDialogModule } from '@angular/material/dialog';
import { provideRouter } from '@angular/router';

describe('FileSubmissionComponent', () => {
  let component: FileSubmissionComponent;
  let fixture: ComponentFixture<FileSubmissionComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, MatDialogModule, FileSubmissionComponent],
      providers: [provideRouter([])]
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(FileSubmissionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
