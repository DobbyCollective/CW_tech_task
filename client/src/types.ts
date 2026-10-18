export interface ManufacturerRecord {
  manufacturerKey: number;
  manufacturerName: string;
}

export interface WeightCategory {
  weightKey: number;
  categoryName: string;
  minimumKg: number;
  maximumKg: number | null;
  iconIdentifier: string;
}

export interface VehicleRecord {
  vehicleIdentifier: number;
  ownerFirstName: string;
  ownerLastName: string;
  ownerOrganisation: string;
  ownerDisplayName: string;
  manufacturerKey: number;
  manufacturerName: string;
  manufacturedYear: number;
  vehicleWeight: number;
  categoryName: string;
  iconIdentifier: string;
}

export interface CreateVehicleRequest {
  ownerFirstName: string;
  ownerLastName: string;
  ownerOrganisation: string;
  manufacturerKey: number;
  manufacturedYear: number;
  vehicleWeight: number;
}

export interface UpsertCategoryRequest {
  categoryName: string;
  iconIdentifier: string;
  minimumKg: number;
  maximumKg: number | null;
}

export type SortField = 'ownerName' | 'manufacturer' | 'year' | 'weight';
export type SortDirection = 'asc' | 'desc';
export type ActiveTab = 'vehicles' | 'categories';
