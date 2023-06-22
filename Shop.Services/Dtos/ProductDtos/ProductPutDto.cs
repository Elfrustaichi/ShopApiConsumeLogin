﻿using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ShopNT.Services.Dtos.ProductDtos
{
    public class ProductPutDto
    {
        public string Name { get; set; }

        public decimal SalePrice { get; set; }

        public decimal CostPrice { get; set; }

        public decimal DiscountPercent { get; set; }
        
        public IFormFile ImageFile { get; set; }

        public int BrandId { get; set; }
    }

    public class ProductPutDtoValidator:AbstractValidator<ProductPutDto>
    {
        public ProductPutDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(20).MinimumLength(2);
            RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(x => x.CostPrice);
            RuleFor(x => x.CostPrice).GreaterThanOrEqualTo(0);
            RuleFor(x => x.DiscountPercent).GreaterThanOrEqualTo(0).LessThanOrEqualTo(100);

            RuleFor(x => x).Custom((x, context) =>
            {
                if (x.DiscountPercent > 0)
                {
                    var price = x.SalePrice * (100 - x.DiscountPercent) / 100;
                    if (x.CostPrice > price)
                    {
                        context.AddFailure(nameof(x.DiscountPercent), "DiscountPercent is incorrect");
                    }
                }

                if (x.ImageFile != null)
                {
                    if (x.ImageFile.Length > 2097152)
                        context.AddFailure(nameof(x.ImageFile), "ImageFile must be less or equal than 2MB");

                    if (x.ImageFile.ContentType != "image/jpeg" && x.ImageFile.ContentType != "image/png")
                        context.AddFailure(nameof(x.ImageFile), "ImageFile must be image/jpeg or image/png");
                }
            });

        }
    }
}
