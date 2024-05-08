import { FormControl, FormArray, FormGroup } from '@angular/forms';
import { SelfServeSupport, SelfServeSupportType, SupportDay } from 'src/app/core/api/models';
import { ETransferNotificationPreference } from 'src/app/core/model/e-transfer-notification-preference.model';

export type SelfServeSupportFormControl = {
  [k in keyof SelfServeSupport]: FormControl<SelfServeSupport[k]>;
};

export interface BaseSelfServeSupportForm extends Omit<SelfServeSupportFormControl, '$type'> {
  totalAmount: FormControl<number>;
}

export interface SupportPersonForm {
  personId: FormControl<string>;
  isSelected?: FormControl<boolean>;
}

export interface SupportPersonDateForm {
  date: FormControl<moment.Moment>;
  personId: FormControl<string>;
  isSelected?: FormControl<boolean>;
}

export type SupportDayFormControl = {
  [k in keyof SupportDay]: FormControl<SupportDay[k]>;
};

export interface SelfServeSupportDayMealForm extends Omit<SupportDayFormControl, 'date'> {
  date: FormControl<moment.Moment>;
  breakfast: FormControl<boolean>;
  lunch: FormControl<boolean>;
  dinner: FormControl<boolean>;
}

export type FundsFor = SelfServeSupportType.FoodGroceries | SelfServeSupportType.FoodRestaurant | null;

export interface SelfServeFoodRestaurantSupportForm {
  includedHouseholdMembers: FormArray<FormGroup<SupportPersonForm>>;
  mealTypes: FormArray<FormGroup<SelfServeSupportDayMealForm>>;
  totalAmount: FormControl<number>;
}

export interface SelfServeFoodGroceriesSupportForm {
  nights: FormArray<FormGroup<SupportPersonDateForm>>;
  totalAmount: FormControl<number>;
}

export interface SelfServeFoodSupportForm {
  fundsFor: FormControl<FundsFor>;
  restaurant: FormGroup<SelfServeFoodRestaurantSupportForm>;
  groceries: FormGroup<SelfServeFoodGroceriesSupportForm>;
}

export interface SelfServeShelerAllowanceSupportForm extends BaseSelfServeSupportForm {
  nights: FormArray<FormGroup<SupportPersonDateForm>>;
}

export interface SelfServeClothingSupportForm extends BaseSelfServeSupportForm {
  includedHouseholdMembers: FormArray<FormGroup<SupportPersonForm>>;
}

export interface SelfServeIncidentsSupportForm extends BaseSelfServeSupportForm {
  includedHouseholdMembers: FormArray<FormGroup<SupportPersonForm>>;
}

export interface DraftSupportForm {
  shelterAllowance: FormGroup<SelfServeShelerAllowanceSupportForm>;
  food: FormGroup<SelfServeFoodSupportForm>;
  clothing: FormGroup<SelfServeClothingSupportForm>;
  incidents: FormGroup<SelfServeIncidentsSupportForm>;
  totals: FormControl<number>;
}

export interface ETransferDetailsForm {
  notificationPreference: FormControl<ETransferNotificationPreference>;
  eTransferEmail: FormControl<string>;
  confirmEmail: FormControl<string>;
  contactEmail: FormControl<string>;
  confirmContactEmail: FormControl<string>;
  useEmailOnFile: FormControl<boolean>;
  eTransferMobile: FormControl<string>;
  confirmMobile: FormControl<string>;
  useMobileOnFile: FormControl<boolean>;
  recipientName: FormControl<string>;
}
