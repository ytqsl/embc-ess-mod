import { Component, OnInit, Inject, OnDestroy } from '@angular/core';
import {
  UntypedFormBuilder,
  UntypedFormGroup,
  AbstractControl,
  UntypedFormControl,
  NgForm,
  FormGroupDirective
} from '@angular/forms';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule } from '@angular/forms';
import { FormCreationService } from 'src/app/core/services/formCreation.service';
import { Subscription } from 'rxjs';

import { CustomValidationService } from 'src/app/core/services/customValidation.service';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatRadioChange, MatRadioModule } from '@angular/material/radio';
import { distinctUntilChanged } from 'rxjs/operators';
import { IMaskDirective } from 'angular-imask';

export class CustomErrorMailMatcher implements ErrorStateMatcher {
  isErrorState(control: UntypedFormControl | null, form: FormGroupDirective | NgForm | null): boolean {
    const isSubmitted = form && form.submitted;
    return (
      !!(control && control.invalid && (control.dirty || control.touched || isSubmitted)) ||
      control.parent.hasError('emailMatch')
    );
  }
}

@Component({
  selector: 'app-contact-info',
  templateUrl: './contact-info.component.html',
  styleUrls: ['./contact-info.component.scss'],
  standalone: true,
  imports: [MatCardModule, ReactiveFormsModule, MatRadioModule, MatFormFieldModule, MatInputModule, IMaskDirective]
})
export default class ContactInfoComponent implements OnInit, OnDestroy {
  contactInfoForm: UntypedFormGroup;
  formBuilder: UntypedFormBuilder;
  contactInfoForm$: Subscription;
  formCreationService: FormCreationService;

  readonly phoneMask = { mask: '000-000-0000' };
  emailMatcher = new CustomErrorMailMatcher();

  constructor(
    @Inject('formBuilder') formBuilder: UntypedFormBuilder,
    @Inject('formCreationService') formCreationService: FormCreationService,
    public customValidator: CustomValidationService
  ) {
    this.formBuilder = formBuilder;
    this.formCreationService = formCreationService;
  }

  ngOnInit(): void {
    this.contactInfoForm$ = this.formCreationService.getContactDetailsForm().subscribe((contactInfo) => {
      this.contactInfoForm = contactInfo;
      this.contactInfoForm.setValidators([this.customValidator.confirmEmailValidator().bind(this.customValidator)]);
      this.contactInfoForm.updateValueAndValidity();
    });

    this.contactInfoForm
      .get('phone')
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        if (value === '') {
          this.contactInfoForm.get('phone').reset();
        }
        this.contactInfoForm.get('email').updateValueAndValidity();
        this.contactInfoForm.get('confirmEmail').updateValueAndValidity();
      });

    this.contactInfoForm
      .get('email')
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        if (value === '') {
          this.contactInfoForm.get('email').reset();
          this.contactInfoForm.get('confirmEmail').reset();
          this.contactInfoForm.get('confirmEmail').disable();
        } else {
          this.contactInfoForm.get('confirmEmail').enable();
        }
        this.contactInfoForm.get('phone').updateValueAndValidity();
        this.contactInfoForm.get('confirmEmail').updateValueAndValidity();
      });

    this.contactInfoForm
      .get('confirmEmail')
      .valueChanges.pipe(distinctUntilChanged())
      .subscribe((value) => {
        if (value === '') {
          this.contactInfoForm.get('confirmEmail').reset();
        }
        this.contactInfoForm.get('email').updateValueAndValidity();
        this.contactInfoForm.get('phone').updateValueAndValidity();
      });
  }

  /**
   * Returns the control of the form
   */
  get contactFormControl(): { [key: string]: AbstractControl } {
    return this.contactInfoForm.controls;
  }

  hideContact(event: MatRadioChange): void {
    if (!event.value) {
      this.contactInfoForm.get('phone').reset();
      this.contactInfoForm.get('email').reset();
      this.contactInfoForm.get('confirmEmail').reset();
      this.updateOnVisibility();
    }
  }

  updateOnVisibility(): void {
    this.contactInfoForm.get('phone').updateValueAndValidity();
    this.contactInfoForm.get('email').updateValueAndValidity();
    this.contactInfoForm.get('confirmEmail').updateValueAndValidity();
  }

  ngOnDestroy(): void {
    this.contactInfoForm$.unsubscribe();
  }
}
