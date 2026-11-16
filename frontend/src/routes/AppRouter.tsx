import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import MainLayout from '../shared/layouts/MainLayout';
import AdminLayout from '../features/admin/layouts/AdminLayout';
import HomePage from '../features/catalog/pages/HomePage';
import LoginPage from '../features/auth/pages/LoginPage';
import RegisterPage from '../features/auth/pages/RegisterPage';
import CategoryManagementPage from '../features/admin/pages/CategoryManagementPage';
import ProductManagementPage from '../features/admin/pages/ProductManagementPage';
import AdminDashboardPage from '../features/admin/pages/AdminDashboardPage';
import OrderManagementPage from '../features/admin/pages/OrderManagementPage';
import InventoryManagementPage from '../features/admin/pages/InventoryManagementPage';
import UserManagementPage from '../features/admin/pages/UserManagementPage';
import ProductListPage from '../features/catalog/pages/ProductListPage';
import ProductDetailPage from '../features/catalog/pages/ProductDetailPage';
import ProfilePage from '../features/profile/pages/ProfilePage';
import CartPage from '../features/cart/pages/CartPage';
import CheckoutPage from '../features/orders/pages/CheckoutPage';
import AddressesPage from '../features/profile/pages/AddressesPage';
import OrderHistoryPage from '../features/orders/pages/OrderHistoryPage';
import OrderDetailPage from '../features/orders/pages/OrderDetailPage';
import MockBankPage from '../features/orders/pages/MockBankPage';
import AuthProvider from '../features/auth/context/AuthContext';
import { CartProvider } from '../features/cart/context/CartContext';
import RequireAdmin from '../features/auth/guards/RequireAdmin';
import RequireCustomer from '../features/auth/guards/RequireCustomer';
import ChatBot from '../features/chat/components/ChatBot';

export default function AppRouter() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <CartProvider>
        <Routes>
        {/* Public layout with Header/Footer */}
        <Route element={<MainLayout />}>
          <Route path="/" element={<HomePage />} />
          <Route path="/products" element={<ProductListPage />} />
          <Route path="/products/:id" element={<ProductDetailPage />} />
          <Route element={<RequireCustomer />}>
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/checkout" element={<CheckoutPage />} />
            <Route path="/orders" element={<OrderHistoryPage />} />
            <Route path="/orders/:id" element={<OrderDetailPage />} />
            <Route path="/mock-bank/orders/:id" element={<MockBankPage />} />
            <Route path="/cart" element={<CartPage />} />
            <Route path="/addresses" element={<AddressesPage />} />
          </Route>
        </Route>

        {/* Auth pages (no header/footer) */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Admin Routes */}
        <Route element={<RequireAdmin />}>
          <Route path="/admin" element={<AdminLayout />}>
            <Route index element={<AdminDashboardPage />} />
            <Route path="orders" element={<OrderManagementPage />} />
            <Route path="inventory" element={<InventoryManagementPage />} />
            <Route path="categories" element={<CategoryManagementPage />} />
            <Route path="products" element={<ProductManagementPage />} />
            <Route path="users" element={<UserManagementPage />} />
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        <ChatBot />
        </CartProvider>
      </AuthProvider>
    </BrowserRouter>
  );
}
