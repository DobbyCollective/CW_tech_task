import React, { useState } from 'react';
import type { UpsertCategoryRequest, WeightCategory } from '../types';
import { CategoryIcon, ICON_OPTIONS } from '../assets/CategoryIcon';


interface CategoryListSectionProps {
  categories: WeightCategory[];
  onUpdateCategory: (weightKey: number, payload: UpsertCategoryRequest) => Promise<boolean>;
  onDeleteCategory: (weightKey: number, categoryName: string) => Promise<void>;
}


export const CategoryListSection: React.FC<CategoryListSectionProps> = ({
  categories,
  onUpdateCategory,
  onDeleteCategory,
}) => {
  const [editingWeightKey, setEditingWeightKey] = useState<number | null>(null);
  const [editCategoryName, setEditCategoryName] = useState<string>('');
  const [editIconIdentifier, setEditIconIdentifier] = useState<string>('car');
  const [editMinimumKg, setEditMinimumKg] = useState<string>('0');
  const [editMaximumKg, setEditMaximumKg] = useState<string>('');


  const startEditingCategory = (cat: WeightCategory): void => {
    setEditingWeightKey(cat.weightKey);
    setEditCategoryName(cat.categoryName);
    setEditIconIdentifier(cat.iconIdentifier);
    setEditMinimumKg(cat.minimumKg.toString());
    setEditMaximumKg(cat.maximumKg !== null ? cat.maximumKg.toString() : '');
  };


  const handleSaveEdit = async (weightKey: number): Promise<void> => {
    const payload: UpsertCategoryRequest = {
      categoryName: editCategoryName.trim(),
      iconIdentifier: editIconIdentifier,
      minimumKg: Number(editMinimumKg),
      maximumKg: editMaximumKg.trim() === '' ? null : Number(editMaximumKg),
    };


    const updated: boolean = await onUpdateCategory(weightKey, payload);
    if (updated) {
      setEditingWeightKey(null);
    }
  };


  return (
    <section className="panel-card category-list-section">
      <h2 className="section-title">Current Weight Categories</h2>
      <p className="helper-note">
        Minimum is inclusive (<code>&gt;=</code>) and Maximum is exclusive (<code>&lt;</code>). Editing a boundary
        automatically adjusts the shared boundary of the adjacent category.
      </p>


      <div className="table-scroll-wrapper">
        <table className="data-table">
          <thead>
            <tr>
              <th>Icon</th>
              <th>Category Name</th>
              <th>Min Weight (kg)</th>
              <th>Max Weight (kg)</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {categories.map((cat: WeightCategory, idx: number) => {
              const isEditing: boolean = editingWeightKey === cat.weightKey;
              const isFirst: boolean = idx === 0;
              const isLast: boolean = idx === categories.length - 1;


              return (
                <tr key={cat.weightKey}>
                  <td>
                    {isEditing ? (
                      <select
                        className="table-inline-input"
                        value={editIconIdentifier}
                        onChange={(e) => setEditIconIdentifier(e.target.value)}
                      >
                        {ICON_OPTIONS.map((opt) => (
                          <option key={opt.value} value={opt.value}>
                            {opt.label}
                          </option>
                        ))}
                      </select>
                    ) : (
                      <CategoryIcon iconIdentifier={cat.iconIdentifier} />
                    )}
                  </td>
                  <td className="cell-wrap">
                    {isEditing ? (
                      <input
                        className="table-inline-input"
                        type="text"
                        value={editCategoryName}
                        onChange={(e) => setEditCategoryName(e.target.value)}
                      />
                    ) : (
                      <strong>{cat.categoryName}</strong>
                    )}
                  </td>
                  <td>
                    {isEditing ? (
                      <input
                        className="table-inline-input"
                        type="number"
                        step="0.01"
                        value={editMinimumKg}
                        disabled={isFirst}
                        onChange={(e) => setEditMinimumKg(e.target.value)}
                        title={isFirst ? 'Lowest category must start at 0 kg' : ''}
                      />
                    ) : (
                      `${cat.minimumKg.toFixed(2)} kg`
                    )}
                  </td>
                  <td>
                    {isEditing ? (
                      <input
                        className="table-inline-input"
                        type="number"
                        step="0.01"
                        value={editMaximumKg}
                        disabled={isLast}
                        placeholder="∞ (And above)"
                        onChange={(e) => setEditMaximumKg(e.target.value)}
                        title={isLast ? 'Highest category is unbounded' : ''}
                      />
                    ) : cat.maximumKg !== null ? (
                      `Up to ${cat.maximumKg.toFixed(2)} kg`
                    ) : (
                      'And above (∞)'
                    )}
                  </td>
                  <td>
                    <div className="action-btn-group">
                      {isEditing ? (
                        <>
                          <button
                            type="button"
                            className="btn-small btn-primary"
                            onClick={() => void handleSaveEdit(cat.weightKey)}
                          >
                            Save
                          </button>
                          <button
                            type="button"
                            className="btn-small"
                            onClick={() => setEditingWeightKey(null)}
                          >
                            Cancel
                          </button>
                        </>
                      ) : (
                        <>
                          <button
                            type="button"
                            className="btn-small"
                            onClick={() => startEditingCategory(cat)}
                          >
                            Edit
                          </button>
                          <button
                            type="button"
                            className="btn-small btn-danger"
                            disabled={categories.length <= 1}
                            onClick={() => void onDeleteCategory(cat.weightKey, cat.categoryName)}
                          >
                            Delete
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </section>
  );
};
