'use client';

import React from 'react';
import Image from 'next/image';
import { Shield, Award, Truck, Gift, Clock, HeadphonesIcon } from 'lucide-react';

type BenefitProps = {
  icon: React.ReactNode;
  title: string;
  description: string;
};

const Benefit = ({ icon, title, description }: BenefitProps) => {
  return (
    <div className="flex items-start space-x-3 mb-6">
      <div className="flex-shrink-0 text-white">
        {icon}
      </div>
      <div>
        <h4 className="text-white font-medium mb-1">{title}</h4>
        <p className="text-blue-100 text-sm">{description}</p>
      </div>
    </div>
  );
};

const WhyThinkPro = () => {
  const benefits = [
    {
      icon: <Shield size={24} />,
      title: "Bảo hành ProCare+",
      description: "Bảo hành tận nơi trong vòng 24h với đội ngũ kỹ thuật chuyên nghiệp",
    },
    {
      icon: <Award size={24} />,
      title: "Cam kết chính hãng",
      description: "Tất cả sản phẩm được phân phối bởi ThinkPro đều là hàng chính hãng",
    },
    {
      icon: <Truck size={24} />,
      title: "Giao hàng miễn phí",
      description: "Giao hàng tận nơi, miễn phí với đơn hàng trên 2 triệu đồng",
    },
    {
      icon: <Gift size={24} />,
      title: "Quà tặng hấp dẫn",
      description: "Nhiều quà tặng giá trị khi mua sản phẩm tại ThinkPro",
    },
    {
      icon: <Clock size={24} />,
      title: "Đổi trả 30 ngày",
      description: "Đổi trả sản phẩm trong vòng 30 ngày nếu có lỗi từ nhà sản xuất",
    },
    {
      icon: <HeadphonesIcon size={24} />,
      title: "Hỗ trợ 24/7",
      description: "Đội ngũ tư vấn viên hỗ trợ khách hàng trong suốt thời gian sử dụng",
    },
  ];

  return (
    <section className="mb-10 py-10 rounded-lg bg-blue-dark">
      <div className="container mx-auto">
        <div className="grid grid-cols-1 md:grid-cols-5 gap-8">
          <div className="md:col-span-3 px-6">
            <h2 className="text-2xl font-bold text-white mb-6">
              Chọn ThinkPro, Chọn sự yên tâm
            </h2>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {benefits.map((benefit, index) => (
                <Benefit 
                  key={index}
                  icon={benefit.icon}
                  title={benefit.title}
                  description={benefit.description}
                />
              ))}
            </div>
          </div>
          
          <div className="md:col-span-2 px-6 flex items-center justify-center">
            <div className="relative h-80 w-full">
              <Image 
                src="/assets/why-thinkpro.png" 
                alt="ThinkPro Customer Support"
                fill
                className="object-contain"
              />
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default WhyThinkPro;
