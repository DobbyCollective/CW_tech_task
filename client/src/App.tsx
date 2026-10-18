import React, { useEffect, useState, useCallback } from 'react';
import type {
  ActiveTab,
  CreateVehicleRequest,
  ManufacturerRecord,
  SortDirection,
  SortField,
  UpsertCategoryRequest,
  VehicleRecord,
  WeightCategory,
} from './types';
import { HeaderSection } from './components/HeaderSection';
import { StatusBannerSection } from './components/StatusBannerSection';
import { AddVehicleSection } from './components/AddVehicleSection';
import { VehicleListSection } from './components/VehicleListSection';
import { CreateCategorySection } from './components/CreateCategorySection';
import { CategoryListSection } from './components/CategoryListSection';
import './App.css';


export function App(): React.ReactElement {
  const [activeTab, setActiveTab] = useState<ActiveTab>('vehicles');

  const [manufacturers, setManufacturers] = useState<ManufacturerRecord[]>([]);
  const [categories, setCategories] = useState<WeightCategory[]>([]);
  const [vehicles, setVehicles] = useState<VehicleRecord[]>([]);
  const [errorBanner, setErrorBanner] = useState<string | null>(null);
  const [successBanner, setSuccessBanner] = useState<string | null>(null);

  const [sortBy, setSortBy] = useState<SortField>('ownerName');
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');

  const clearMessages = (): void => {
    setErrorBanner(null);
    setSuccessBanner(null);
  };

  const fetchManufacturersAndCategories = useCallback(async (): Promise<void> => {
    try {
      const [mfgRes, catRes] = await Promise.all([
        fetch('/api/manufacturers'),
        fetch('/api/categories'),
      ]);

      if (mfgRes.ok) {
        const mfgData: ManufacturerRecord[] = await mfgRes.json();
        setManufacturers(mfgData);
      }

      if (catRes.ok) {
        const catData: WeightCategory[] = await catRes.json();
        setCategories(catData);
      }
    } catch {
      setErrorBanner('Unable to connect to the Motorvehicle API. Ensure the backend is running.');
    }
  }, []);

  const fetchVehicles = useCallback(async (): Promise<void> => {
    try {
      const response: Response = await fetch(
        `/api/vehicles?sortBy=${encodeURIComponent(sortBy)}&sortDirection=${encodeURIComponent(sortDirection)}`
      );
      if (response.ok) {
        const data: VehicleRecord[] = await response.json();
        setVehicles(data);
      }
    } catch {
      setErrorBanner('Failed to load vehicle list from the server.');
    }
  }, [sortBy, sortDirection]);

  useEffect(() => {
    void fetchManufacturersAndCategories();
  }, [fetchManufacturersAndCategories]);

  useEffect(() => {
    void fetchVehicles();
  }, [fetchVehicles]);

  const handleSelectTab = (tab: ActiveTab): void => {
    clearMessages();
    setActiveTab(tab);
  };

  const handleSortClick = (field: SortField): void => {
    if (sortBy === field) {
      setSortDirection((prev: SortDirection) => (prev === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortBy(field);
      setSortDirection('asc');
    }
  };

  const handleCreateVehicle = async (payload: CreateVehicleRequest): Promise<boolean> => {
    clearMessages();

    const response: Response = await fetch('/api/vehicles', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const errBody: { message?: string } = await response.json().catch(() => ({}));
      setErrorBanner(errBody.message ?? 'Failed to create vehicle. Please check your inputs.');
      return false;
    }

    setSuccessBanner('Vehicle added and categorised.');
    await fetchVehicles();
    return true;
  };

  const handleCreateCategory = async (payload: UpsertCategoryRequest): Promise<boolean> => {
    clearMessages();

    const response: Response = await fetch('/api/categories', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const errBody: { message?: string } = await response.json().catch(() => ({}));
      setErrorBanner(errBody.message ?? 'Failed to create category.');
      return false;
    }

    setSuccessBanner('New weight category created and boundaries updated.');
    await fetchManufacturersAndCategories();
    await fetchVehicles();
    return true;
  };

  const handleUpdateCategory = async (
    weightKey: number,
    payload: UpsertCategoryRequest
  ): Promise<boolean> => {
    clearMessages();

    const response: Response = await fetch(`/api/categories/${weightKey}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const errBody: { message?: string } = await response.json().catch(() => ({}));
      setErrorBanner(errBody.message ?? 'Failed to update category.');
      return false;
    }

    setSuccessBanner('Category updated and existing vehicles recategorised automatically.');
    await fetchManufacturersAndCategories();
    await fetchVehicles();
    return true;
  };

  const handleDeleteCategory = async (weightKey: number, categoryName: string): Promise<void> => {
    clearMessages();

    const response: Response = await fetch(`/api/categories/${weightKey}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      const errBody: { message?: string } = await response.json().catch(() => ({}));
      setErrorBanner(errBody.message ?? `Cannot delete category '${categoryName}'.`);
      return;
    }

    setSuccessBanner(
      `Deleted '${categoryName}' and merged its weight range into the adjacent category.`
    );
    await fetchManufacturersAndCategories();
    await fetchVehicles();
  };

  return (
    <div className="app-shell">
      <HeaderSection
        activeTab={activeTab}
        vehicleCount={vehicles.length}
        categoryCount={categories.length}
        onSelectTab={handleSelectTab}
      />


      <StatusBannerSection
        errorBanner={errorBanner}
        successBanner={successBanner}
        onDismiss={clearMessages}
      />


      {activeTab === 'vehicles' ? (
        <main className="workspace-grid vehicles-workspace-grid">
          <AddVehicleSection
            manufacturers={manufacturers}
            onCreateVehicle={handleCreateVehicle}
          />
          <VehicleListSection
            vehicles={vehicles}
            sortBy={sortBy}
            sortDirection={sortDirection}
            onSortClick={handleSortClick}
          />
        </main>
      ) : (
        <main className="workspace-grid categories-workspace-grid">
          <CreateCategorySection onCreateCategory={handleCreateCategory} />
          <CategoryListSection
            categories={categories}
            onUpdateCategory={handleUpdateCategory}
            onDeleteCategory={handleDeleteCategory}
          />
        </main>
      )}
    </div>
  );
}

export default App;
