/* tslint:disable */
/* eslint-disable */
import { Support } from './support';
import { SupportCategory } from './support-category';
import { SupportSubCategory } from './support-sub-category';
export type FoodRestaurantSupport = Support & {
'category'?: SupportCategory;
'subCategory'?: SupportSubCategory;
'numberOfBreakfastsPerPerson'?: number;
'numberOfLunchesPerPerson'?: number;
'numberOfDinnersPerPerson'?: number;
'totalAmount'?: number;
};
