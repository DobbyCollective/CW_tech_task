import React from 'react';
import type { SortDirection, SortField, VehicleRecord } from '../types';
import { CategoryIcon } from '../assets/CategoryIcon';


interface VehicleListSectionProps {
  vehicles: VehicleRecord[];
  sortBy: SortField;
  sortDirection: SortDirection;
  onSortClick: (field: SortField) => void;
}


export const VehicleListSection: React.FC<VehicleListSectionProps> = ({
  vehicles,
  sortBy,
  sortDirection,
  onSortClick,
}) => {
  const renderSortIndicator = (field: SortField): string => {
    if (sortBy !== field) {
      return '↕';
    }
    return sortDirection === 'asc' ? '▲ (Asc)' : '▼ (Desc)';
  };


  return (
    <section className="panel-card vehicle-list-section">
      <div className="section-header-bar">
        <h2 className="section-title">Registered Vehicles</h2>
        <span className="sort-status-pill">
          Sorted by: <strong>{sortBy}</strong> ({sortDirection.toUpperCase()})
        </span>
      </div>


      <div className="table-scroll-wrapper">
        <table className="data-table">
          <thead>
            <tr>
              <th>
                <button
                  type="button"
                  className={sortBy === 'ownerName' ? 'sort-btn active' : 'sort-btn'}
                  onClick={() => onSortClick('ownerName')}
                >
                  Owner&apos;s Name {renderSortIndicator('ownerName')}
                </button>
              </th>
              <th>
                <button
                  type="button"
                  className={sortBy === 'manufacturer' ? 'sort-btn active' : 'sort-btn'}
                  onClick={() => onSortClick('manufacturer')}
                >
                  Manufacturer {renderSortIndicator('manufacturer')}
                </button>
              </th>
              <th>
                <button
                  type="button"
                  className={sortBy === 'year' ? 'sort-btn active' : 'sort-btn'}
                  onClick={() => onSortClick('year')}
                >
                  Year {renderSortIndicator('year')}
                </button>
              </th>
              <th>
                <button
                  type="button"
                  className={sortBy === 'weight' ? 'sort-btn active' : 'sort-btn'}
                  onClick={() => onSortClick('weight')}
                >
                  Weight (kg) {renderSortIndicator('weight')}
                </button>
              </th>
              <th>Category</th>
            </tr>
          </thead>
          <tbody>
            {vehicles.length === 0 ? (
              <tr>
                <td colSpan={5} className="empty-row">
                  No vehicles recorded yet. Use the form on the left to add one.
                </td>
              </tr>
            ) : (
              vehicles.map((v: VehicleRecord) => (
                <tr key={v.vehicleIdentifier}>
                  <td className="cell-wrap">{v.ownerDisplayName}</td>
                  <td>{v.manufacturerName}</td>
                  <td>{v.manufacturedYear}</td>
                  <td>{v.vehicleWeight.toFixed(2)} kg</td>
                  <td>
                    <CategoryIcon
                      iconIdentifier={v.iconIdentifier}
                      categoryName={v.categoryName}
                    />
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </section>
  );
};
