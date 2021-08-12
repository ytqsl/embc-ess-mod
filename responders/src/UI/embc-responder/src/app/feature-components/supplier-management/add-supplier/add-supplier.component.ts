import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { CustomValidationService } from 'src/app/core/services/customValidation.service';
import { AlertService } from 'src/app/shared/components/alert/alert.service';
import { GstModel } from '../suppliers-list/supplier-list-data.service';
import { AddSupplierService } from './add-supplier.service';
import * as globalConst from '../../../core/services/global-constants';

@Component({
  selector: 'app-add-supplier',
  templateUrl: './add-supplier.component.html',
  styleUrls: ['./add-supplier.component.scss']
})
export class AddSupplierComponent implements OnInit {
  addForm: FormGroup;
  showLoader = false;

  constructor(
    private builder: FormBuilder,
    private customValidation: CustomValidationService,
    private router: Router,
    private addSupplierService: AddSupplierService,
    private alertService: AlertService
  ) {}

  ngOnInit(): void {
    this.constructAddForm();
  }

  constructAddForm(): void {
    this.addForm = this.builder.group({
      supplierLegalName: ['', [this.customValidation.whitespaceValidator()]],
      supplierName: [''],
      gstNumber: this.builder.group(
        {
          part1: [
            '',
            [Validators.required, Validators.pattern(/^-?(0|[1-9]\d*)?$/)]
          ],
          part2: [
            '',
            [Validators.required, Validators.pattern(/^-?(0|[1-9]\d*)?$/)]
          ]
        },
        {
          validators: [
            this.customValidation.groupRequiredValidator(),
            this.customValidation.groupMinLengthValidator()
          ]
        }
      )
    });
  }

  /**
   * Returns form control
   */
  get addFormControl(): { [key: string]: AbstractControl } {
    return this.addForm.controls;
  }

  next(): void {
    // this.saveDataForm();
    // this.router.navigate([
    //   '/responder-access/supplier-management/new-supplier'
    // ]);
    const gstNumber: GstModel = {
      part1: this.addForm.get('gstNumber.part1').value,
      part2: this.addForm.get('gstNumber.part2').value
    };
    this.checkSupplierExistance(gstNumber);
  }

  cancel(): void {
    this.router.navigate([
      '/responder-access/supplier-management/list-suppliers'
    ]);
  }

  get gstNumber(): FormGroup {
    return this.addForm.get('gstNumber') as FormGroup;
  }

  /**
   * Checks if the supplier exists in the ERA system
   *
   * @param $event username input change event
   */
  private checkSupplierExistance(gstNumber: GstModel): void {
    this.showLoader = !this.showLoader;
    // this.addSupplierService.checkSupplierExists(gstNumber).subscribe(
    //   (value) => {
    //     this.showLoader = !this.showLoader;
    //     console.log(value);
    //   },
    //   (error) => {
    //     console.log(error);
    //     this.showLoader = !this.showLoader;
    //     this.alertService.clearAlert();
    //     this.alertService.setAlert('danger', globalConst.supplierCheckerror);
    //   }
    // );
    if (true) {
      this.router.navigate([
        '/responder-access/supplier-management/supplier-exist'
      ]);
    } else {
      this.saveDataForm();
      this.router.navigate([
        '/responder-access/supplier-management/new-supplier'
      ]);
    }
  }

  private saveDataForm() {
    this.addSupplierService.supplierLegalName = this.addForm.get(
      'supplierLegalName'
    ).value;
    this.addSupplierService.supplierName = this.addForm.get(
      'supplierName'
    ).value;

    const gstNumber: GstModel = {
      part1: this.addForm.get('gstNumber.part1').value,
      part2: this.addForm.get('gstNumber.part2').value
    };
    this.addSupplierService.gstNumber = gstNumber;
  }
}
