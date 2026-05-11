import apiClient from './apiClient';

export interface Category {
  id: number;
  name: string;
  description?: string;
  imageUrl?: string;
}

export interface Medicine {
  id: number;
  name: string;
  scientificName?: string;
  activeIngredient?: string;
  imageUrl?: string;
  defaultSalePrice: number;
  defaultPurchasePrice?: number;
  categoryId?: number;
  categoryName?: string;
  manufacturer?: string;
  notes?: string;
  totalStock?: number;
  status?: string;
  internalCode?: string;
  defaultBarcode?: string;
}

export const CatalogService = {
  async getCategories(): Promise<Category[]> {
    const response = await apiClient.get('/categories');
    // Extracting items from paginated response
    return response.data.data.items || response.data.data;
  },

  async getMedicines(categoryId?: number, search?: string): Promise<Medicine[]> {
    const params = new URLSearchParams();
    if (categoryId) params.append('categoryId', categoryId.toString());
    if (search) params.append('search', search);

    const response = await apiClient.get(`/medicines?${params.toString()}`);
    // Extracting the items from paginated response
    return response.data.data.items || response.data.data;
  },

  async getMedicineDetails(id: number): Promise<Medicine> {
    const response = await apiClient.get(`/medicines/${id}`);
    return response.data.data;
  }
};
