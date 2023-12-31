﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using ShopNT.Core.Entities;
using ShopNT.Core.Repositories;
using ShopNT.Services.Dtos.Common;
using ShopNT.Services.Dtos.ProductDtos;
using ShopNT.Services.Exceptions;
using ShopNT.Services.Helpers;
using ShopNT.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ShopNT.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IBrandRepository _brandRepository;
        private readonly IHttpContextAccessor _accessor;

        public ProductService(IProductRepository productRepository,IMapper mapper,IBrandRepository brandRepository, IHttpContextAccessor accessor)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _brandRepository = brandRepository;
            _accessor = accessor;
        }
        public CreatedEntityDto Create(ProductPostDto dto)
        {
            List<RestExceptionError> errors = new List<RestExceptionError>();

            if (!_brandRepository.IsExist(x => x.Id == dto.BrandId))
                errors.Add(new RestExceptionError("BrandId", "BrandId is not correct"));

            if (_productRepository.IsExist(x => x.Name == dto.Name))
                errors.Add(new RestExceptionError("Name", "Name is already exists"));

            if (errors.Count > 0) throw new RestException(System.Net.HttpStatusCode.BadRequest, errors);

            var entity = _mapper.Map<Product>(dto);

            string rootPath = Directory.GetCurrentDirectory() + "/wwwroot";
            entity.ImageName = FileManager.Save(dto.ImageFile, rootPath, "uploads/products");

            _productRepository.Add(entity);
            _productRepository.Commit();

            return new CreatedEntityDto { Id = entity.Id };
        }

        public void Delete(int id)
        {
            var entity = _productRepository.Get(x => x.Id == id);

            if (entity == null) throw new RestException(System.Net.HttpStatusCode.NotFound, "Product not found");

            _productRepository.Delete(entity);
            _productRepository.Commit();

            string rootPath = Directory.GetCurrentDirectory() + "/wwwroot";
            FileManager.Delete(rootPath, "uplods/products", entity.ImageName);
        }

        public void Edit(int id, ProductPutDto dto)
        {
            var entity = _productRepository.Get(x => x.Id == id);

            if (entity == null) throw new RestException(System.Net.HttpStatusCode.NotFound, "Product not found");

            List<RestExceptionError> errors = new List<RestExceptionError>();

            if (!_brandRepository.IsExist(x => x.Id == dto.BrandId))
                errors.Add(new RestExceptionError("BrandId", "BrandId is not correct"));

            if (dto.Name != entity.Name && _productRepository.IsExist(x => x.Name == dto.Name))
                errors.Add(new RestExceptionError("Name", "Name is already exists"));

            entity.CostPrice = dto.CostPrice;
            entity.Name = dto.Name;
            entity.SalePrice = dto.SalePrice;
            entity.DiscountPercent = dto.DiscountPercent;
            entity.BrandId = dto.BrandId;

            string? removableFileName = null;
            string rootPath = Directory.GetCurrentDirectory() + "/wwwroot";

            if (dto.ImageFile != null)
            {
                removableFileName = entity.ImageName;
                entity.ImageName = FileManager.Save(dto.ImageFile, rootPath, "uploads/products");
            }

            _productRepository.Commit();

            if (removableFileName != null) FileManager.Delete(rootPath, "uploads/products", removableFileName);
        }

        public List<ProductGetAllItemsDto> GetAll()
        {
            var entities = _productRepository.GetAll(x => true, "Brand");

            return _mapper.Map<List<ProductGetAllItemsDto>>(entities);
        }

        

        public ProductGetItemDto GetById(int id)
        {
            var entity = _productRepository.Get(x => x.Id == id, "Brand");

            if (entity == null) throw new RestException(System.Net.HttpStatusCode.NotFound, "Product not found");

            return _mapper.Map<ProductGetItemDto>(entity);
        }
    }
}
