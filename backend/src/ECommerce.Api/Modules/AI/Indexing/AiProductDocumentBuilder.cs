using System.Security.Cryptography;
using System.Text;
using ECommerce.Api.Modules.Catalog.Contracts;

namespace ECommerce.Api.Modules.AI.Indexing;

public static class AiProductDocumentBuilder
{
    public static (string Content, string Hash) Build(ProductIndexSource product)
    {
        var content = new StringBuilder()
            .AppendLine($"Sản phẩm: {product.ProductName}")
            .AppendLine($"SKU: {product.Sku}")
            .AppendLine($"Thương hiệu: {product.BrandName}")
            .AppendLine($"Danh mục: {product.CategoryName}")
            .AppendLine($"Mô tả: {product.Description ?? "Chưa có mô tả."}")
            .AppendLine($"Thông số: {product.Specifications ?? "Chưa có thông số."}")
            .ToString()
            .Trim();

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
        return (content, hash);
    }
}
