import React, { useState } from 'react';
import type { UpsertCategoryRequest } from '../types';
import { ICON_OPTIONS } from '../assets/CategoryIcon';


interface CreateCategorySectionProps {
  onCreateCategory: (payload: UpsertCategoryRequest) => Promise<boolean>;
}


export const CreateCategorySection: React.FC<CreateCategorySectionProps> = ({
  onCreateCategory,
}) => {
  const [newCategoryName, setNewCategoryName] = useState<string>('');
  const [newIconIdentifier, setNewIconIdentifier] = useState<string>('van');
  const [newMinimumKg, setNewMinimumKg] = useState<string>('');
  const [newMaximumKg, setNewMaximumKg] = useState<string>('');


  const handleSubmit = async (e: React.FormEvent): Promise<void> => {
    e.preventDefault();


    const payload: UpsertCategoryRequest = {
      categoryName: newCategoryName.trim(),
      iconIdentifier: newIconIdentifier,
      minimumKg: Number(newMinimumKg),
      maximumKg: newMaximumKg.trim() === '' ? null : Number(newMaximumKg),
    };


    const created: boolean = await onCreateCategory(payload);
    if (created) {
      setNewCategoryName('');
      setNewMinimumKg('');
      setNewMaximumKg('');
    }
  };


  return (
    <section className="panel-card category-form-section">
      <h2 className="section-title">Create New Category</h2>
      <p className="helper-note">
        To ensure coverage without gaps or overlaps, a new category must align
        with the minimum or maximum boundary of an existing category (i.e. splitting it), or leave
        Maximum Weight blank to add a new heaviest category at the top. Lowest category must start from 0.00kg. 
      </p>
      <form onSubmit={(e) => void handleSubmit(e)} className="form-flex-stack">
        <div className="field-group">
          <label htmlFor="newCatName">Category Name *</label>
          <input
            id="newCatName"
            type="text"
            value={newCategoryName}
            onChange={(e) => setNewCategoryName(e.target.value)}
            placeholder="e.g. Ultra Heavy"
            required
          />
        </div>


        <div className="field-group">
          <label htmlFor="newCatIcon">Category Icon *</label>
          <select
            id="newCatIcon"
            value={newIconIdentifier}
            onChange={(e) => setNewIconIdentifier(e.target.value)}
          >
            {ICON_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>


        <div className="field-grid-two-col">
          <div className="field-group">
            <label htmlFor="newMinKg">Minimum Weight (kg) *</label>
            <input
              id="newMinKg"
              type="number"
              step="0.01"
              min="0"
              value={newMinimumKg}
              onChange={(e) => setNewMinimumKg(e.target.value)}
              placeholder="e.g. 5000"
              required
            />
          </div>
          <div className="field-group">
            <label htmlFor="newMaxKg">Maximum Weight (kg, blank = ∞)</label>
            <input
              id="newMaxKg"
              type="number"
              step="0.01"
              min="0.01"
              value={newMaximumKg}
              onChange={(e) => setNewMaximumKg(e.target.value)}
              placeholder="Blank for unbounded"
            />
          </div>
        </div>


        <button type="submit" className="btn-primary">
          Create Category
        </button>
      </form>
    </section>
  );
};
