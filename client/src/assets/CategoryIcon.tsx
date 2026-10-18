import React from 'react';


export interface IconOption {
  value: string;
  label: string;
}


export const ICON_OPTIONS: IconOption[] = [
  { value: 'car', label: 'Hatchback / Sedan (Car)' },
  { value: 'suv', label: 'SUV / Medium Utility' },
  { value: 'van', label: 'Commercial Van' },
  { value: 'truck', label: 'Heavy Truck' },
  { value: 'heavy-truck', label: 'Freight Truck' },
];


interface CategoryIconProps {
  iconIdentifier: string;
  categoryName?: string;
}


export const CategoryIcon: React.FC<CategoryIconProps> = ({
  iconIdentifier,
  categoryName,
}) => {
  const renderSvg = (): React.ReactNode => {
    switch (iconIdentifier.toLowerCase()) {
      case 'car':
        return (
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M5 17h14v-5l-2-5H7L5 12v5z" />
            <circle cx="7.5" cy="17.5" r="1.5" />
            <circle cx="16.5" cy="17.5" r="1.5" />
          </svg>
        );
      case 'suv':
        return (
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M3 16h18v-5l-3-5H6L3 11v5z" />
            <circle cx="7" cy="17" r="2" />
            <circle cx="17" cy="17" r="2" />
          </svg>
        );
      case 'van':
        return (
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="2" y="7" width="16" height="9" rx="1" />
            <path d="M18 10h3l1 3v3h-4v-6z" />
            <circle cx="6.5" cy="17" r="2" />
            <circle cx="17.5" cy="17" r="2" />
          </svg>
        );
      case 'truck':
      case 'heavy-truck':
      default:
        return (
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" strokeWidth="2">
            <rect x="1" y="6" width="14" height="10" />
            <path d="M15 9h4l3 4v3h-7V9z" />
            <circle cx="5.5" cy="17.5" r="2" />
            <circle cx="18.5" cy="17.5" r="2" />
          </svg>
        );
    }
  };


  return (
    <span className="category-badge" title={categoryName ?? iconIdentifier}>
      {renderSvg()}
      {categoryName && <span className="category-badge-text">{categoryName}</span>}
    </span>
  );
};
